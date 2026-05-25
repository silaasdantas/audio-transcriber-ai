namespace AudioTranscriberAI.Api.Features.Transcriptions.Shared;

public sealed record AudioUpload(
    string JobId,
    string OriginalFileName,
    string? ContentType,
    string Extension,
    long SizeBytes,
    string StoredPath);
