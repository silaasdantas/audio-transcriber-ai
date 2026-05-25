namespace AudioTranscriberAI.Api.Features.Transcriptions.Shared;

public sealed record DownloadArtifact(
    string JobId,
    TranscriptKind Type,
    string FileName,
    string ContentType,
    string Content);
