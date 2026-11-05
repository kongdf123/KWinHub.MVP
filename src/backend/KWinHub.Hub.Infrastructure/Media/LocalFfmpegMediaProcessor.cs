using System.Diagnostics;
using KWinHub.Hub.Application.Media;
using Microsoft.Extensions.Logging;

namespace KWinHub.Hub.Infrastructure.Media;

public class LocalFfmpegMediaProcessor : IMediaProcessor
{
    private readonly ILogger<LocalFfmpegMediaProcessor> _logger;
    private readonly string _ffmpegPath;
    private bool? _isAvailable;

    public string ProviderName => "local-ffmpeg";

    public LocalFfmpegMediaProcessor(ILogger<LocalFfmpegMediaProcessor> logger)
    {
        _logger = logger;
        _ffmpegPath = Environment.GetEnvironmentVariable("KWINHUB_FFMPEG_PATH") ?? "ffmpeg";
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (_isAvailable.HasValue)
        {
            return _isAvailable.Value;
        }

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);
            _isAvailable = process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "ffmpeg is not available at {FfmpegPath}", _ffmpegPath);
            _isAvailable = false;
        }

        return _isAvailable.Value;
    }

    public async Task<MediaProcessingResult> ProcessAsync(
        MediaProcessingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await IsAvailableAsync(cancellationToken))
        {
            return new MediaProcessingResult(
                Success: true,
                ProviderId: ProviderName,
                Skipped: true,
                SkipReason: "ffmpeg not available");
        }

        try
        {
            var outputKey = $"{request.StorageKey}.processed.mp4";
            var arguments = $"-i {request.StorageKey} -c:v libx264 -preset fast -crf 23 -c:a aac -movflags +faststart {outputKey}";

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync(cancellationToken);
                return new MediaProcessingResult(
                    Success: false,
                    ProviderId: ProviderName,
                    Error: $"ffmpeg exited with code {process.ExitCode}: {error}");
            }

            return new MediaProcessingResult(
                Success: true,
                ProviderId: ProviderName,
                PlaybackUrl: outputKey);
        }
        catch (Exception ex)
        {
            return new MediaProcessingResult(
                Success: false,
                ProviderId: ProviderName,
                Error: ex.Message);
        }
    }
}