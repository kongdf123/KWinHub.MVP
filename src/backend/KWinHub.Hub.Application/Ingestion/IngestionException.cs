namespace KWinHub.Hub.Application.Ingestion;

public class IngestionException : Exception
{
    public string ErrorCode { get; }
    public string? Field { get; }

    public IngestionException(string errorCode, string message, string? field = null)
        : base(message)
    {
        ErrorCode = errorCode;
        Field = field;
    }
}