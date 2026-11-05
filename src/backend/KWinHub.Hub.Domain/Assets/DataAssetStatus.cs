namespace KWinHub.Hub.Assets;

public enum DataAssetStatus
{
    PendingUpload,
    UploadInProgress,
    UploadedUnverified,
    UploadedVerified,
    UploadFailed,
    VerificationFailed,
    Processing,
    Ready,
    ProcessingFailed
}