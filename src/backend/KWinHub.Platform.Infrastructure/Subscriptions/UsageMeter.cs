using KWinHub.Application.Subscriptions;
using KWinHub.Domain.Subscriptions;
using KWinHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Infrastructure.Subscriptions;

public class UsageMeter : IUsageMeter
{
    private readonly KWinHubDbContext _db;
    private readonly ISubscriptionRepository _repository;
    private readonly TimeProvider _clock;

    public UsageMeter(KWinHubDbContext db, ISubscriptionRepository repository, TimeProvider clock)
    {
        _db = db;
        _repository = repository;
        _clock = clock;
    }

    public async Task<long> ConsumeAsync(
        Guid organizationId,
        string metricKey,
        string businessEventId,
        long delta,
        CancellationToken cancellationToken = default)
    {
        const int maxRetries = 3;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                return await ConsumeOnceAsync(organizationId, metricKey, businessEventId, delta, cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (attempt == maxRetries - 1) throw;
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // Concurrent duplicate event id; retry will find the existing row.
                if (attempt == maxRetries - 1) throw;
            }
        }

        throw new InvalidOperationException("Usage metering failed after retries.");
    }

    private async Task<long> ConsumeOnceAsync(
        Guid organizationId,
        string metricKey,
        string businessEventId,
        long delta,
        CancellationToken cancellationToken)
    {
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1) Idempotency: if this business event already exists, no-op.
            var existing = await _db.UsageLedgerEntries
                .FirstOrDefaultAsync(
                    l => l.OrganizationId == organizationId
                         && l.MetricKey == metricKey
                         && l.BusinessEventId == businessEventId,
                    cancellationToken);

            if (existing is not null)
            {
                await transaction.CommitAsync(cancellationToken);
                return existing.Delta;
            }

            // 2) Resolve effective limit.
            var limit = await GetEffectiveLimitAsync(organizationId, metricKey, cancellationToken);

            // 3) Resolve period window.
            var (periodStart, periodEnd) = ResolvePeriod(metricKey);

            // 4) Load or create aggregate for this period.
            var aggregate = await _db.UsageAggregates
                .FirstOrDefaultAsync(
                    a => a.OrganizationId == organizationId
                         && a.MetricKey == metricKey
                         && a.PeriodStart == periodStart,
                    cancellationToken);

            if (aggregate is null)
            {
                aggregate = new UsageAggregate(organizationId, metricKey, periodStart, periodEnd, consumed: 0);
                await _db.UsageAggregates.AddAsync(aggregate, cancellationToken);
            }

            // 5) Check quota.
            if (limit.HasValue && aggregate.Consumed + delta > limit.Value)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new QuotaExceededException(metricKey, aggregate.Consumed, limit, delta);
            }

            // 6) Insert ledger + update aggregate.
            var occurredAt = _clock.GetUtcNow();
            var entry = new UsageLedgerEntry(organizationId, metricKey, businessEventId, delta, occurredAt, periodStart);
            await _db.UsageLedgerEntries.AddAsync(entry, cancellationToken);
            aggregate.Add(delta);

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return delta;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<long> GetConsumedAsync(
        Guid organizationId, string metricKey, CancellationToken cancellationToken = default)
    {
        var (periodStart, _) = ResolvePeriod(metricKey);
        var aggregate = await _db.UsageAggregates
            .FirstOrDefaultAsync(
                a => a.OrganizationId == organizationId
                     && a.MetricKey == metricKey
                     && a.PeriodStart == periodStart,
                cancellationToken);
        return aggregate?.Consumed ?? 0;
    }

    public async Task<long?> GetEffectiveLimitAsync(
        Guid organizationId, string metricKey, CancellationToken cancellationToken = default)
    {
        long? limit = null;

        var subscription = await _repository.GetActiveSubscriptionAsync(organizationId, cancellationToken);
        if (subscription is not null)
        {
            var plan = await _repository.GetPlanAsync(subscription.PlanId, cancellationToken);
            var planQuota = plan?.Quotas.FirstOrDefault(q => q.QuotaKey == metricKey);
            limit = planQuota?.LimitValue;
        }

        // Override wins if currently effective.
        var now = _clock.GetUtcNow();
        var overrides = await _repository.GetQuotaOverridesAsync(organizationId, cancellationToken);
        var effectiveOverride = overrides
            .Where(o => o.QuotaKey == metricKey && o.IsEffective(now))
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefault();

        if (effectiveOverride is not null)
        {
            limit = effectiveOverride.LimitValue;
        }

        return limit;
    }

    private (DateTimeOffset Start, DateTimeOffset End) ResolvePeriod(string metricKey)
    {
        var now = _clock.GetUtcNow();
        if (metricKey == SubscriptionCatalog.QuotaSessionsMonthly)
        {
            var start = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var end = start.AddMonths(1);
            return (start, end);
        }

        // Non-monthly quotas: rolling window from epoch to far future.
        return (DateTimeOffset.UnixEpoch, DateTimeOffset.MaxValue);
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message?.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true
               || ex.InnerException?.Message?.Contains("23505", StringComparison.OrdinalIgnoreCase) == true;
    }
}
