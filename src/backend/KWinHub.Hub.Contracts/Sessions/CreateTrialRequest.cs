namespace KWinHub.Hub.Contracts.Sessions;

public record CreateTrialRequest(
    Guid SessionId,
    int TrialIndex,
    string? ExternalId = null,
    string? Notes = null);