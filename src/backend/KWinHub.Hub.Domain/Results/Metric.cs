using KWinHub.BuildingBlocks.Common;
using KWinHub.Hub.Protocols;

namespace KWinHub.Hub.Results;

public class Metric : BaseEntity<Guid>
{
    public Guid ResultId { get; private set; }

    public Guid? MetricDefinitionId { get; private set; }

    public string MetricCode { get; private set; } = string.Empty;

    public string MetricName { get; private set; } = string.Empty;

    public MetricValueType ValueType { get; private set; } = MetricValueType.Scalar;

    public double? ScalarValue { get; private set; }

    public double? LeftValue { get; private set; }

    public double? RightValue { get; private set; }

    public string? CategoricalValue { get; private set; }

    public string? StructuredValue { get; private set; }

    public string Unit { get; private set; } = string.Empty;

    public string? Side { get; private set; }

    public double Value => ScalarValue ?? 0;

    private Metric()
    {
    }

    public Metric(
        Guid resultId,
        string metricCode,
        string metricName,
        double value,
        string unit,
        string? side = null,
        Guid? metricDefinitionId = null)
    {
        Id = Guid.NewGuid();
        ResultId = resultId;
        MetricDefinitionId = metricDefinitionId;
        MetricCode = metricCode;
        MetricName = metricName;
        ValueType = MetricValueType.Scalar;
        ScalarValue = value;
        Unit = unit;
        Side = side;
    }

    public static Metric CreateBilateral(
        Guid resultId,
        string metricCode,
        string metricName,
        double leftValue,
        double rightValue,
        string unit,
        Guid? metricDefinitionId = null)
    {
        return new Metric
        {
            Id = Guid.NewGuid(),
            ResultId = resultId,
            MetricDefinitionId = metricDefinitionId,
            MetricCode = metricCode,
            MetricName = metricName,
            ValueType = MetricValueType.Bilateral,
            LeftValue = leftValue,
            RightValue = rightValue,
            Unit = unit
        };
    }

    public static Metric CreateCategorical(
        Guid resultId,
        string metricCode,
        string metricName,
        string categoricalValue,
        string unit,
        Guid? metricDefinitionId = null)
    {
        return new Metric
        {
            Id = Guid.NewGuid(),
            ResultId = resultId,
            MetricDefinitionId = metricDefinitionId,
            MetricCode = metricCode,
            MetricName = metricName,
            ValueType = MetricValueType.Categorical,
            CategoricalValue = categoricalValue,
            Unit = unit
        };
    }

    public static Metric CreateStructured(
        Guid resultId,
        string metricCode,
        string metricName,
        string structuredValue,
        string unit,
        Guid? metricDefinitionId = null)
    {
        return new Metric
        {
            Id = Guid.NewGuid(),
            ResultId = resultId,
            MetricDefinitionId = metricDefinitionId,
            MetricCode = metricCode,
            MetricName = metricName,
            ValueType = MetricValueType.Structured,
            StructuredValue = structuredValue,
            Unit = unit
        };
    }
}