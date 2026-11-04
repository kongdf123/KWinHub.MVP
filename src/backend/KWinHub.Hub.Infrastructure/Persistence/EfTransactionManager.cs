using KWinHub.Hub.Application.Common;
using KWinHub.Hub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace KWinHub.Hub.Infrastructure.Persistence;

public class EfTransactionManager : ITransactionManager
{
    private readonly HubDbContext _dbContext;

    public EfTransactionManager(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ITransactionScope> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new EfTransactionScope(transaction);
    }

    private sealed class EfTransactionScope : ITransactionScope
    {
        private readonly IDbContextTransaction _transaction;

        public EfTransactionScope(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await _transaction.CommitAsync(cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            await _transaction.RollbackAsync(cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _transaction.DisposeAsync();
        }
    }
}