namespace KWinHub.Hub.Contracts.Sessions;

public record CreateDataSourceRequest(
    string Name,
    string Code,
    string SourceApp,
    string? Version = null,
    string? Description = null);