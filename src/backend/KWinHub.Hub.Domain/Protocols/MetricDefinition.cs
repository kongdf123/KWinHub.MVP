using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Protocols;

public class MetricDefinition : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public MetricValueType ValueType { get; private set; }

    public string Unit { get; private set; } = string.Empty;

    public string Version { get; private set; } = "1.0";

    public string? Method { get; private set; }

    public string? AlgorithmVersion { get; private set; }

    public string? Category { get; private set; }

    public string? Description { get; private set; }

    public string Status { get; private set; } = "Active";

    private MetricDefinition()
    {
    }

    public MetricDefinition(
        Guid organizationId,
        string code,
        string name,
        MetricValueType valueType,
        string unit,
        string version = "1.0",
        string? method = null,
        string? algorithmVersion = null,
        string? category = null,
        string? description = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Code = code;
        Name = name;
        ValueType = valueType;
        Unit = unit;
        Version = version;
        Method = method;
        AlgorithmVersion = algorithmVersion;
        Category = category;
        Description = description;
    }

    public void Update(
        string name,
        MetricValueType valueType,
        string unit,
        string version,
        string? method = null,
        string? algorithmVersion = null,
        string? category = null,
        string? description = null)
    {
        Name = name;
        ValueType = valueType;
        Unit = unit;
        Version = version;
        Method = method;
        AlgorithmVersion = algorithmVersion;
        Category = category;
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deprecate()
    {
        Status = "Deprecated";
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}