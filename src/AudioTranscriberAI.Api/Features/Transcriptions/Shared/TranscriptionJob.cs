namespace AudioTranscriberAI.Api.Features.Transcriptions.Shared;

public sealed record TranscriptionJob(
    string Id,
    string OriginalFileName,
    string StoredFileName,
    string Format,
    long SizeBytes,
    TranscriptionStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string? RawTranscriptPath = null,
    string? ImprovedTranscriptPath = null,
    string? FailureCode = null,
    string? FailureMessage = null);
