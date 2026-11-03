namespace KWinHub.Contracts.Sessions;

public record CreateSessionRequest(
    Guid SubjectId,
    Guid ProtocolId,
    string ProtocolVersion = "1.0",
    Guid? SourceId = null,
    Guid? DeviceId = null,
    string? ClientApplication = null,
    string? ClientVersion = null,
    string? ExternalId = null,
    string? Timezone = null);
