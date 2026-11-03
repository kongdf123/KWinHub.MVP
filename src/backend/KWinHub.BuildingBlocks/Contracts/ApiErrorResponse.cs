namespace KWinHub.BuildingBlocks.Contracts;

public record ApiErrorResponse(
    string Code,
    string Message,
    string? TraceId = null,
    object? Details = null);