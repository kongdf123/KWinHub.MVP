using KWinHub.Hub.Contracts.Sessions;
using KWinHub.Hub.Sessions;

namespace KWinHub.Hub.Application.Sessions;

public class DataSourceService : IDataSourceService
{
    private readonly IDataSourceRepository _repository;

    public DataSourceService(IDataSourceRepository repository)
    {
        _repository = repository;
    }

    public async Task<DataSourceResponse> CreateAsync(
        Guid organizationId,
        CreateDataSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        var source = new DataSource(
            organizationId: organizationId,
            name: request.Name,
            code: request.Code,
            sourceApp: request.SourceApp,
            version: request.Version,
            description: request.Description);

        await _repository.AddAsync(source, cancellationToken);
        return ToResponse(source);
    }

    public async Task<DataSourceResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var source = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        return source is null ? null : ToResponse(source);
    }

    public async Task<IReadOnlyList<DataSourceResponse>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var sources = await _repository.ListByOrganizationAsync(organizationId, cancellationToken);
        return sources.Select(ToResponse).ToList();
    }

    public async Task<DataSourceResponse?> DeactivateAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var source = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        if (source is null) return null;

        source.Deactivate();
        await _repository.UpdateAsync(source, cancellationToken);
        return ToResponse(source);
    }

    private static DataSourceResponse ToResponse(DataSource source) =>
        new(
            Id: source.Id,
            OrganizationId: source.OrganizationId,
            Name: source.Name,
            Code: source.Code,
            SourceApp: source.SourceApp,
            Version: source.Version,
            Description: source.Description,
            Status: source.Status,
            CreatedAt: source.CreatedAt,
            UpdatedAt: source.UpdatedAt);
}