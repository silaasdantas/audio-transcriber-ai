namespace AudioTranscriberAI.Api.Features.Transcriptions.Shared;

public interface IAudioProcessor
{
    Task<Result<PreparedAudio>> PrepareAsync(
        AudioUpload upload,
        CancellationToken cancellationToken);
}

public sealed record PreparedAudio(
    string JobId,
    string Path,
    string Format,
    IReadOnlyList<string> ChunkPaths);
