using KWinHub.Hub.Contracts.Plans;

namespace KWinHub.Hub.Application.Plans;

public static class ExhibitionPlanFixture
{
    public const string ExhibitionPlanCode = "EXHIBITION-JUMP-2026";
    public const string ExhibitionPlanName = "Exhibition Jump Assessment Battery";

    public static CreateAssessmentPlanRequest CreateDefaultRequest()
    {
        return new CreateAssessmentPlanRequest(
            Name: ExhibitionPlanName,
            Code: ExhibitionPlanCode,
            Description: "Fixture-defined exhibition plan covering countermovement and drop jump metrics. " +
                         "Replace when D-01 metric catalog freezes.",
            ProtocolId: null,
            ProtocolCode: "JUMP",
            Items: new[]
            {
                new CreateAssessmentPlanItemRequest(
                    MetricCode: "cmj_height",
                    MetricName: "Countermovement Jump Height",
                    Unit: "cm",
                    ValueType: "Scalar",
                    Required: true,
                    MinValue: 0,
                    MaxValue: 120),
                new CreateAssessmentPlanItemRequest(
                    MetricCode: "cmj_peak_power",
                    MetricName: "CMJ Peak Power",
                    Unit: "W",
                    ValueType: "Scalar",
                    Required: true,
                    MinValue: 0,
                    MaxValue: null),
                new CreateAssessmentPlanItemRequest(
                    MetricCode: "rsi_modified",
                    MetricName: "Modified Reactive Strength Index",
                    Unit: "m/s",
                    ValueType: "Scalar",
                    Required: false,
                    MinValue: 0,
                    MaxValue: null),
                new CreateAssessmentPlanItemRequest(
                    MetricCode: "drop_jump_height",
                    MetricName: "Drop Jump Height",
                    Unit: "cm",
                    ValueType: "Scalar",
                    Required: false,
                    MinValue: 0,
                    MaxValue: 120)
            });
    }
}