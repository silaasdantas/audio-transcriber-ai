namespace AudioTranscriberAI.Api.Features.Transcriptions.Shared;

public interface ITranscriptImprover
{
    Task<Result<string>> ImproveAsync(
        string rawTranscript,
        CancellationToken cancellationToken);
}
