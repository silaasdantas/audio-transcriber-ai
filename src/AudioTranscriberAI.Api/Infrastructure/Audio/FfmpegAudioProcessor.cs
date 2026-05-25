using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using AudioTranscriberAI.Api.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AudioTranscriberAI.Api.Infrastructure.Audio;

public sealed class FfmpegAudioProcessor(IOptions<TranscriptionOptions> options) : IAudioProcessor
{
    private readonly TranscriptionOptions _options = options.Value;

    public Task<Result<PreparedAudio>> PrepareAsync(
        AudioUpload upload,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.FfmpegPath))
        {
            return Task.FromResult(Result<PreparedAudio>.Failure(TranscriptionError.Processing(
                "ffmpeg.not_configured",
                "The audio processor is not configured.")));
        }

        if (!File.Exists(upload.StoredPath))
        {
            return Task.FromResult(Result<PreparedAudio>.Failure(TranscriptionError.Processing(
                "audio.file_missing",
                "The uploaded audio file could not be found.")));
        }

        var prepared = new PreparedAudio(
            upload.JobId,
            upload.StoredPath,
            upload.Extension,
            Array.Empty<string>());

        return Task.FromResult(Result<PreparedAudio>.Success(prepared));
    }
}
