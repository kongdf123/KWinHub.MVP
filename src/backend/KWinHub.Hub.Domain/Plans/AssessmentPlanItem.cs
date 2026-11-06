using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Plans;

public class AssessmentPlanItem : BaseEntity<Guid>
{
    public Guid PlanId { get; private set; }

    public string MetricCode { get; private set; } = string.Empty;

    public string MetricName { get; private set; } = string.Empty;

    public string Unit { get; private set; } = string.Empty;

    public string ValueType { get; private set; } = "Scalar";

    public bool Required { get; private set; }

    public double? MinValue { get; private set; }

    public double? MaxValue { get; private set; }

    public int SortOrder { get; private set; }

    private AssessmentPlanItem()
    {
    }

    public AssessmentPlanItem(
        Guid planId,
        string metricCode,
        string metricName,
        string unit,
        string valueType,
        bool required,
        double? minValue = null,
        double? maxValue = null)
    {
        Id = Guid.NewGuid();
        PlanId = planId;
        MetricCode = metricCode;
        MetricName = metricName;
        Unit = unit;
        ValueType = valueType;
        Required = required;
        MinValue = minValue;
        MaxValue = maxValue;
    }

    internal void SetSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
    }
}