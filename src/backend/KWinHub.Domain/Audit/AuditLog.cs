using KWinHub.Domain.Common;

namespace KWinHub.Domain.Audit;

public class AuditLog : BaseEntity<Guid>
{
    public Guid? ActorUserId { get; private set; }

    public string? ActorEmail { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string EntityType { get; private set; } = string.Empty;

    public string? EntityId { get; private set; }

    public string Summary { get; private set; } = string.Empty;

    public string? MetadataJson { get; private set; }

    public string? IpAddress { get; private set; }

    private AuditLog()
    {
    }

    public AuditLog(
        string action,
        string entityType,
        string? entityId,
        string summary,
        Guid? actorUserId = null,
        string? actorEmail = null,
        string? metadataJson = null,
        string? ipAddress = null)
    {
        Id = Guid.NewGuid();
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        Summary = summary;
        ActorUserId = actorUserId;
        ActorEmail = actorEmail;
        MetadataJson = metadataJson;
        IpAddress = ipAddress;
    }
}
