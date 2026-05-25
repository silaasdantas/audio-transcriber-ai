namespace AudioTranscriberAI.Api.Features.Transcriptions.Shared;

public sealed record TranscriptArtifact(
    string JobId,
    TranscriptKind Kind,
    string Path,
    DateTimeOffset CreatedAtUtc,
    int LengthCharacters);

public enum TranscriptKind
{
    Raw,
    Improved
}
