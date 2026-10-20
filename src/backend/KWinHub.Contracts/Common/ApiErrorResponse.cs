namespace KWinHub.Contracts.Common;

public record ApiErrorResponse(
    string Code,
    string Message,
    string? TraceId = null,
    object? Details = null);