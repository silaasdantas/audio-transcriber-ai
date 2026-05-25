namespace AudioTranscriberAI.Api.Features.Transcriptions.Shared;

public interface ILocalFileStorage
{
    Task<Result<AudioUpload>> SaveOriginalAsync(
        string jobId,
        string originalFileName,
        string? contentType,
        Stream content,
        long sizeBytes,
        CancellationToken cancellationToken);

    Task<Result<TranscriptArtifact>> SaveTranscriptAsync(
        string jobId,
        TranscriptKind kind,
        string content,
        CancellationToken cancellationToken);

    Task<Result<string>> ReadTranscriptAsync(
        string jobId,
        TranscriptKind kind,
        CancellationToken cancellationToken);
}
