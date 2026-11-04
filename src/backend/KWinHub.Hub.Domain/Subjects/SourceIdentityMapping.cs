using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Subjects;

public class SourceIdentityMapping : BaseEntity<Guid>
{
    public const string StatusResolved = "Resolved";
    public const string StatusConflict = "Conflict";

    public Guid OrganizationId { get; private set; }

    public string SourceApp { get; private set; } = string.Empty;

    public string InstallationScope { get; private set; } = string.Empty;

    public string ExternalId { get; private set; } = string.Empty;

    public Guid SubjectId { get; private set; }

    public string Status { get; private set; } = StatusResolved;

    public Guid? ResolvedBy { get; private set; }

    public DateTimeOffset? ResolvedAt { get; private set; }

    public string? ConflictReason { get; private set; }

    private SourceIdentityMapping()
    {
    }

    public SourceIdentityMapping(
        Guid organizationId,
        string sourceApp,
        string installationScope,
        string externalId,
        Guid subjectId)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        SourceApp = sourceApp;
        InstallationScope = installationScope;
        ExternalId = externalId;
        SubjectId = subjectId;
        Status = StatusResolved;
    }

    public bool IsResolved => Status == StatusResolved;

    public bool IsConflict => Status == StatusConflict;

    public void MarkConflict(string reason)
    {
        Status = StatusConflict;
        ConflictReason = reason;
        ResolvedBy = null;
        ResolvedAt = null;
    }

    public void Resolve(Guid subjectId, Guid resolvedBy)
    {
        SubjectId = subjectId;
        Status = StatusResolved;
        ResolvedBy = resolvedBy;
        ResolvedAt = DateTimeOffset.UtcNow;
        ConflictReason = null;
    }
}