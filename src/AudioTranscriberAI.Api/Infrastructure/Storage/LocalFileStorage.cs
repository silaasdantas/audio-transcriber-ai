using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using AudioTranscriberAI.Api.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AudioTranscriberAI.Api.Infrastructure.Storage;

public sealed class LocalFileStorage(
    IOptions<TranscriptionOptions> options,
    TimeProvider timeProvider) : ILocalFileStorage
{
    private readonly TranscriptionOptions _options = options.Value;

    public async Task<Result<AudioUpload>> SaveOriginalAsync(
        string jobId,
        string originalFileName,
        string? contentType,
        Stream content,
        long sizeBytes,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(originalFileName).TrimStart('.').ToLowerInvariant();
        var safeFileName = Path.GetFileName(originalFileName);
        var directory = Path.Combine(_options.StorageRoot, jobId, "original");
        Directory.CreateDirectory(directory);

        var storedPath = Path.Combine(directory, safeFileName);
        await using var file = File.Create(storedPath);
        await content.CopyToAsync(file, cancellationToken);

        return Result<AudioUpload>.Success(new AudioUpload(
            jobId,
            safeFileName,
            contentType,
            extension,
            sizeBytes,
            storedPath));
    }

    public async Task<Result<TranscriptArtifact>> SaveTranscriptAsync(
        string jobId,
        TranscriptKind kind,
        string content,
        CancellationToken cancellationToken)
    {
        var fileName = kind == TranscriptKind.Raw ? "raw.txt" : "improved.txt";
        var path = Path.Combine(_options.StorageRoot, jobId, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await File.WriteAllTextAsync(path, content, cancellationToken);

        return Result<TranscriptArtifact>.Success(new TranscriptArtifact(
            jobId,
            kind,
            path,
            timeProvider.GetUtcNow(),
            content.Length));
    }

    public async Task<Result<string>> ReadTranscriptAsync(
        string jobId,
        TranscriptKind kind,
        CancellationToken cancellationToken)
    {
        var fileName = kind == TranscriptKind.Raw ? "raw.txt" : "improved.txt";
        var path = Path.Combine(_options.StorageRoot, jobId, fileName);
        if (!File.Exists(path))
        {
            return Result<string>.Failure(TranscriptionError.Conflict(
                "transcript.not_ready",
                "The requested transcript is not available yet."));
        }

        return Result<string>.Success(await File.ReadAllTextAsync(path, cancellationToken));
    }
}
