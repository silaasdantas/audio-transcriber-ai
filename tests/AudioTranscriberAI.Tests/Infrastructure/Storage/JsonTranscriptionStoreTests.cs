using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using AudioTranscriberAI.Api.Infrastructure.Configuration;
using AudioTranscriberAI.Api.Infrastructure.Storage;
using Microsoft.Extensions.Options;

namespace AudioTranscriberAI.Tests.Infrastructure.Storage;

public sealed class JsonTranscriptionStoreTests
{
    [Fact]
    public async Task Save_and_get_round_trips_job_metadata()
    {
        using var temp = new TempDirectory();
        var store = CreateStore(temp.Path);
        var job = CreateJob("job-1");

        await store.SaveAsync(job, CancellationToken.None);
        var result = await store.GetAsync(job.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(job.Id, result.Value!.Id);
        Assert.Equal(TranscriptionStatus.Pending, result.Value.Status);
    }

    [Fact]
    public async Task Get_returns_not_found_for_unknown_job()
    {
        using var temp = new TempDirectory();
        var store = CreateStore(temp.Path);

        var result = await store.GetAsync("missing", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("transcription.not_found", result.Error!.Code);
    }

    [Fact]
    public async Task Update_changes_existing_job()
    {
        using var temp = new TempDirectory();
        var store = CreateStore(temp.Path);
        var job = CreateJob("job-1");
        await store.SaveAsync(job, CancellationToken.None);

        var result = await store.UpdateAsync(
            job.Id,
            current => current with { Status = TranscriptionStatus.Completed },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TranscriptionStatus.Completed, result.Value!.Status);
    }

    private static JsonTranscriptionStore CreateStore(string storageRoot) =>
        new(Options.Create(new TranscriptionOptions { StorageRoot = storageRoot }));

    private static TranscriptionJob CreateJob(string id) =>
        new(
            id,
            "sample.mp3",
            "sample.mp3",
            "mp3",
            10,
            TranscriptionStatus.Pending,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);
}
