using System.Text;
using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using AudioTranscriberAI.Api.Infrastructure.Configuration;
using AudioTranscriberAI.Api.Infrastructure.Storage;
using Microsoft.Extensions.Options;

namespace AudioTranscriberAI.Tests.Infrastructure.Storage;

public sealed class LocalFileStorageTests
{
    [Fact]
    public async Task SaveOriginalAsync_writes_original_file_under_job_directory()
    {
        using var temp = new TempDirectory();
        var storage = CreateStorage(temp.Path);
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("fake audio"));

        var result = await storage.SaveOriginalAsync(
            "job-1",
            "sample.MP3",
            "audio/mpeg",
            stream,
            stream.Length,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("mp3", result.Value!.Extension);
        Assert.True(File.Exists(result.Value.StoredPath));
    }

    [Fact]
    public async Task SaveTranscriptAsync_and_ReadTranscriptAsync_round_trip_raw_text()
    {
        using var temp = new TempDirectory();
        var storage = CreateStorage(temp.Path);

        var save = await storage.SaveTranscriptAsync(
            "job-1",
            TranscriptKind.Raw,
            "hello world",
            CancellationToken.None);
        var read = await storage.ReadTranscriptAsync("job-1", TranscriptKind.Raw, CancellationToken.None);

        Assert.True(save.IsSuccess);
        Assert.True(read.IsSuccess);
        Assert.Equal("hello world", read.Value);
    }

    private static LocalFileStorage CreateStorage(string storageRoot) =>
        new(Options.Create(new TranscriptionOptions { StorageRoot = storageRoot }), TimeProvider.System);
}
