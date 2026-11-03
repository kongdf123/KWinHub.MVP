using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Results;

public class Metric : BaseEntity<Guid>
{
    public Guid ResultId { get; private set; }

    public string MetricCode { get; private set; } = string.Empty;

    public string MetricName { get; private set; } = string.Empty;

    public double Value { get; private set; }

    public string Unit { get; private set; } = string.Empty;

    public string? Side { get; private set; }

    private Metric()
    {
    }

    public Metric(Guid resultId, string metricCode, string metricName, double value, string unit, string? side = null)
    {
        Id = Guid.NewGuid();
        ResultId = resultId;
        MetricCode = metricCode;
        MetricName = metricName;
        Value = value;
        Unit = unit;
        Side = side;
    }
}