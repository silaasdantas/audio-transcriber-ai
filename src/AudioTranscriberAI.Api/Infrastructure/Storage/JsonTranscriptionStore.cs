using System.Text.Json;
using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using AudioTranscriberAI.Api.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AudioTranscriberAI.Api.Infrastructure.Storage;

public sealed class JsonTranscriptionStore(IOptions<TranscriptionOptions> options) : ITranscriptionStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly TranscriptionOptions _options = options.Value;

    public async Task SaveAsync(TranscriptionJob job, CancellationToken cancellationToken)
    {
        var jobDirectory = GetJobDirectory(job.Id);
        Directory.CreateDirectory(jobDirectory);
        await WriteJobAsync(job, cancellationToken);
    }

    public async Task<Result<TranscriptionJob>> GetAsync(string id, CancellationToken cancellationToken)
    {
        var metadataPath = GetMetadataPath(id);
        if (!File.Exists(metadataPath))
        {
            return Result<TranscriptionJob>.Failure(TranscriptionError.NotFound());
        }

        await using var stream = File.OpenRead(metadataPath);
        var job = await JsonSerializer.DeserializeAsync<TranscriptionJob>(
            stream,
            JsonOptions,
            cancellationToken);

        return job is null
            ? Result<TranscriptionJob>.Failure(TranscriptionError.Processing(
                "storage.metadata_invalid",
                "The transcription metadata could not be read."))
            : Result<TranscriptionJob>.Success(job);
    }

    public async Task<Result<TranscriptionJob>> UpdateAsync(
        string id,
        Func<TranscriptionJob, TranscriptionJob> update,
        CancellationToken cancellationToken)
    {
        var current = await GetAsync(id, cancellationToken);
        if (!current.IsSuccess)
        {
            return current;
        }

        var updated = update(current.Value!);
        await WriteJobAsync(updated, cancellationToken);
        return Result<TranscriptionJob>.Success(updated);
    }

    private async Task WriteJobAsync(TranscriptionJob job, CancellationToken cancellationToken)
    {
        var metadataPath = GetMetadataPath(job.Id);
        var tempPath = $"{metadataPath}.tmp";
        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, job, JsonOptions, cancellationToken);
        }

        File.Move(tempPath, metadataPath, overwrite: true);
    }

    private string GetMetadataPath(string id) =>
        Path.Combine(GetJobDirectory(id), "metadata.json");

    private string GetJobDirectory(string id) =>
        Path.Combine(_options.StorageRoot, id);
}
