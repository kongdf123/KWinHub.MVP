using KWinHub.Domain.Protocols;
using KWinHub.Domain.Sessions;

namespace KWinHub.Domain.Results;

/// <summary>
/// Deterministic simulated analysis for force-plate sessions.
/// Produces reproducible metrics based on session/protocol IDs.
/// </summary>
public static class ResultSimulator
{
    public static Result CreateSimulatedResult(MeasurementSession session, Protocol protocol)
    {
        var result = new Result(
            organizationId: session.OrganizationId,
            sessionId: session.Id,
            protocolId: session.ProtocolId,
            protocolVersion: session.ProtocolVersion,
            algorithmVersion: "1.0.0-demo");

        // Seed RNG from stable IDs so same input always gives same output
        var seed = (int)(session.Id.GetHashCode() ^ protocol.Id.GetHashCode());
        var rng = new Random(seed);

        switch (protocol.Code)
        {
            case "CMJ":
                result.AddMetric("jump_height", "Jump Height", 0.40 + rng.NextDouble() * 0.15, "m");
                result.AddMetric("peak_force", "Peak Force", 1800 + rng.NextDouble() * 500, "N");
                result.AddMetric("peak_power", "Peak Power", 3500 + rng.NextDouble() * 800, "W");
                result.AddMetric("impulse", "Impulse", 220 + rng.NextDouble() * 80, "N·s");
                result.AddMetric("flight_time", "Flight Time", 0.45 + rng.NextDouble() * 0.12, "s");
                break;

            case "IMTP":
                result.AddMetric("peak_force", "Peak Force", 2500 + rng.NextDouble() * 600, "N");
                result.AddMetric("relative_peak_force", "Relative Peak Force", 32 + rng.NextDouble() * 8, "N/kg");
                result.AddMetric("force_at_50ms", "Force at 50ms", 900 + rng.NextDouble() * 300, "N");
                result.AddMetric("force_at_100ms", "Force at 100ms", 1500 + rng.NextDouble() * 400, "N");
                break;

            case "DJ":
                result.AddMetric("jump_height", "Jump Height", 0.35 + rng.NextDouble() * 0.15, "m");
                result.AddMetric("ground_contact_time", "Ground Contact Time", 0.20 + rng.NextDouble() * 0.10, "s");
                result.AddMetric("reactive_strength_index", "Reactive Strength Index", 1.5 + rng.NextDouble() * 1.0, "m/s");
                result.AddMetric("peak_force", "Peak Force", 1600 + rng.NextDouble() * 400, "N");
                break;

            default:
                result.AddMetric("score", "Score", 50 + rng.NextDouble() * 50, "pts");
                break;
        }

        return result;
    }
}
