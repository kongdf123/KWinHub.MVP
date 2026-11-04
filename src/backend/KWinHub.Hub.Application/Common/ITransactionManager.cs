namespace KWinHub.Hub.Application.Common;

public interface ITransactionScope : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

public interface ITransactionManager
{
    Task<ITransactionScope> BeginTransactionAsync(CancellationToken cancellationToken = default);
}