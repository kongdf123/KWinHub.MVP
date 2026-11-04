namespace KWinHub.Hub.Contracts.Ingestion;

public static class IngestionErrorCodes
{
    public const string UnknownSubject = "unknown_subject";
    public const string InvalidProtocol = "invalid_protocol";
    public const string InvalidProtocolVersion = "invalid_protocol_version";
    public const string UnsupportedMetric = "unsupported_metric";
    public const string UnitMismatch = "unit_mismatch";
    public const string InvalidValueType = "invalid_value_type";
    public const string DuplicateSession = "duplicate_session";
    public const string DuplicateTrial = "duplicate_trial";
    public const string Conflict = "conflict";
    public const string ValidationFailed = "validation_failed";
    public const string InternalError = "internal_error";
}