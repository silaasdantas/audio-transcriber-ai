namespace AudioTranscriberAI.Api.Features.Transcriptions.Shared;

public interface ITranscriptionJobProcessor
{
    Task<Result<TranscriptionJob>> StartAsync(
        string originalFileName,
        string? contentType,
        Stream content,
        long sizeBytes,
        CancellationToken cancellationToken);
}
