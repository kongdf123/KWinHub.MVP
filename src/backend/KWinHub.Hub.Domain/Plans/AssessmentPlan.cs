using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Plans;

public class AssessmentPlan : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int Version { get; private set; } = 1;

    public AssessmentPlanStatus Status { get; private set; } = AssessmentPlanStatus.Draft;

    public Guid? ProtocolId { get; private set; }

    public string? ProtocolCode { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    private readonly List<AssessmentPlanItem> _items = new();
    public IReadOnlyList<AssessmentPlanItem> Items => _items.AsReadOnly();

    private AssessmentPlan()
    {
    }

    public AssessmentPlan(
        Guid organizationId,
        string name,
        string code,
        Guid createdByUserId,
        Guid? protocolId = null,
        string? protocolCode = null,
        string? description = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Name = name;
        Code = code;
        CreatedByUserId = createdByUserId;
        ProtocolId = protocolId;
        ProtocolCode = protocolCode;
        Description = description;
    }

    public void SetItems(IEnumerable<AssessmentPlanItem> items)
    {
        if (Status == AssessmentPlanStatus.Published)
        {
            throw new InvalidOperationException("Cannot modify items of a published plan. Create a new version instead.");
        }

        _items.Clear();
        var sortOrder = 0;
        foreach (var item in items)
        {
            item.SetSortOrder(sortOrder++);
            _items.Add(item);
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDetails(string name, string? description, Guid? protocolId, string? protocolCode)
    {
        if (Status == AssessmentPlanStatus.Published)
        {
            throw new InvalidOperationException("Cannot modify a published plan. Create a new version instead.");
        }

        Name = name;
        Description = description;
        ProtocolId = protocolId;
        ProtocolCode = protocolCode;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Publish()
    {
        if (Status != AssessmentPlanStatus.Draft)
        {
            throw new InvalidOperationException($"Cannot publish a plan in {Status} status. Only Draft plans can be published.");
        }

        Status = AssessmentPlanStatus.Published;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Retire()
    {
        if (Status != AssessmentPlanStatus.Published)
        {
            throw new InvalidOperationException($"Cannot retire a plan in {Status} status. Only Published plans can be retired.");
        }

        Status = AssessmentPlanStatus.Retired;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public AssessmentPlan CreateNewVersion(Guid createdByUserId)
    {
        if (Status != AssessmentPlanStatus.Published)
        {
            throw new InvalidOperationException("Can only create a new version from a published plan.");
        }

        var newPlan = new AssessmentPlan(
            OrganizationId,
            Name,
            Code,
            createdByUserId,
            ProtocolId,
            ProtocolCode,
            Description)
        {
            Version = Version + 1,
            Status = AssessmentPlanStatus.Draft
        };

        foreach (var item in _items)
        {
            newPlan._items.Add(new AssessmentPlanItem(
                newPlan.Id,
                item.MetricCode,
                item.MetricName,
                item.Unit,
                item.ValueType,
                item.Required,
                item.MinValue,
                item.MaxValue));
        }

        return newPlan;
    }
}