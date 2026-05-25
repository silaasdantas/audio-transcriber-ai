namespace AudioTranscriberAI.Api.Features.Transcriptions.Shared;

public interface ITranscriptionService
{
    Task<Result<string>> TranscribeAsync(
        PreparedAudio audio,
        CancellationToken cancellationToken);
}
