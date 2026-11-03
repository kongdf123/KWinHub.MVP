namespace KWinHub.Hub.Sessions;

public enum SessionStatus
{
    Received,
    Validating,
    Validated,
    Queued,
    Processing,
    Analyzing,
    Published,
    ValidationFailed,
    ProcessingFailed,
    AnalysisFailed
}