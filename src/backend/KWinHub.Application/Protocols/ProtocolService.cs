using KWinHub.Contracts.Protocols;
using KWinHub.Domain.Protocols;

namespace KWinHub.Application.Protocols;

public class ProtocolService : IProtocolService
{
    private readonly IProtocolRepository _repository;

    public ProtocolService(IProtocolRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ProtocolResponse>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var protocols = await _repository.ListByOrganizationAsync(organizationId, cancellationToken);
        return protocols.Select(ToResponse).ToList();
    }

    public async Task<ProtocolResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var protocol = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        return protocol is null ? null : ToResponse(protocol);
    }

    public async Task<ProtocolResponse?> GetByCodeAndVersionAsync(
        Guid organizationId,
        string code,
        string version,
        CancellationToken cancellationToken = default)
    {
        var protocol = await _repository.GetByCodeAndVersionAsync(organizationId, code, version, cancellationToken);
        return protocol is null ? null : ToResponse(protocol);
    }

    public async Task<ProtocolResponse> CreateAsync(
        Guid organizationId,
        CreateProtocolRequest request,
        CancellationToken cancellationToken = default)
    {
        var protocol = new Protocol(
            organizationId: organizationId,
            name: request.Name,
            code: request.Code,
            version: request.Version,
            category: request.Category,
            sport: request.Sport,
            description: request.Description);

        await _repository.AddAsync(protocol, cancellationToken);

        return ToResponse(protocol);
    }

    private static ProtocolResponse ToResponse(Protocol protocol) =>
        new(
            Id: protocol.Id,
            OrganizationId: protocol.OrganizationId,
            Name: protocol.Name,
            Code: protocol.Code,
            Version: protocol.Version,
            Category: protocol.Category,
            Sport: protocol.Sport,
            Status: protocol.Status.ToString(),
            Description: protocol.Description,
            CreatedAt: protocol.CreatedAt,
            UpdatedAt: protocol.UpdatedAt);
}
