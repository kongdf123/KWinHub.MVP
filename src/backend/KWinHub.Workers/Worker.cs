using KWinHub.Domain.Protocols;
using KWinHub.Domain.Results;
using KWinHub.Domain.Sessions;
using KWinHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Workers;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("KWinHub analysis worker starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOneSessionAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Unhandled error in analysis worker loop.");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task ProcessOneSessionAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KWinHubDbContext>();

        // Pick one session that needs processing — prefer Received > Validating > Queued > Processing
        var session = await db.MeasurementSessions
            .Where(s => s.Status == SessionStatus.Received
                     || s.Status == SessionStatus.Validating
                     || s.Status == SessionStatus.Queued
                     || s.Status == SessionStatus.Processing)
            .OrderBy(s => s.UpdatedAt ?? s.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (session is null) return;

        _logger.LogInformation("Processing session {SessionId} (Status={Status})", session.Id, session.Status);

        // Validating → Validated → Processing → Analyzed → Published
        if (session.Status == SessionStatus.Received || session.Status == SessionStatus.Validating)
        {
            // Verify subject and protocol exist (validation gate)
            var subjectExists = await db.Subjects.AnyAsync(s => s.Id == session.SubjectId, ct);
            var protocol = await db.Protocols.FirstOrDefaultAsync(p => p.Id == session.ProtocolId, ct);

            if (!subjectExists || protocol is null)
            {
                session.TransitionTo(SessionStatus.ValidationFailed);
                await db.SaveChangesAsync(ct);
                _logger.LogWarning("Session {SessionId} validation failed.", session.Id);
                return;
            }

            session.TransitionTo(SessionStatus.Validated);
            session.TransitionTo(SessionStatus.Processing);
            await db.SaveChangesAsync(ct);
        }

        if (session.Status == SessionStatus.Validated || session.Status == SessionStatus.Queued || session.Status == SessionStatus.Processing)
        {
            var protocol = await db.Protocols.FirstOrDefaultAsync(p => p.Id == session.ProtocolId, ct);
            if (protocol is null)
            {
                session.TransitionTo(SessionStatus.ProcessingFailed);
                await db.SaveChangesAsync(ct);
                return;
            }

            // Don't duplicate results
            var existing = await db.Results.AnyAsync(r => r.SessionId == session.Id, ct);
            if (!existing)
            {
                var result = ResultSimulator.CreateSimulatedResult(session, protocol);
                await db.Results.AddAsync(result, ct);
            }

            session.TransitionTo(SessionStatus.Analyzing);
            session.TransitionTo(SessionStatus.Published);
            await db.SaveChangesAsync(ct);

            _logger.LogInformation("Session {SessionId} published with results.", session.Id);
        }
    }
}
