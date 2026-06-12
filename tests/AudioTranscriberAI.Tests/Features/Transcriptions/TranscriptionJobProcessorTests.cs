using System.Text;
using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using AudioTranscriberAI.Api.Features.Transcriptions.UploadTranscription;
using AudioTranscriberAI.Api.Infrastructure.Audio;
using AudioTranscriberAI.Api.Infrastructure.Configuration;
using AudioTranscriberAI.Api.Infrastructure.Storage;
using Microsoft.Extensions.Options;

namespace AudioTranscriberAI.Tests.Features.Transcriptions;

public sealed class TranscriptionJobProcessorTests
{
    [Fact]
    public async Task StartAsync_saves_upload_transcribes_and_persists_raw_transcript()
    {
        using var temp = new TempDirectory();
        var options = Options.Create(new TranscriptionOptions { StorageRoot = temp.Path, MaxUploadBytes = 1000 });
        var store = new JsonTranscriptionStore(options);
        var storage = new LocalFileStorage(options, TimeProvider.System);
        var processor = new TranscriptionJobProcessor(
            options,
            store,
            storage,
            new FfmpegAudioProcessor(options),
            new FakeTranscriptionService("raw transcript"),
            new FakeTranscriptImprover("improved transcript"),
            TimeProvider.System);
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("fake audio"));

        var result = await processor.StartAsync(
            "sample.mp3",
            "audio/mpeg",
            stream,
            stream.Length,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TranscriptionStatus.Completed, result.Value!.Status);

        var raw = await storage.ReadTranscriptAsync(result.Value.Id, TranscriptKind.Raw, CancellationToken.None);
        Assert.True(raw.IsSuccess);
        Assert.Equal("raw transcript", raw.Value);

        var improved = await storage.ReadTranscriptAsync(result.Value.Id, TranscriptKind.Improved, CancellationToken.None);
        Assert.True(improved.IsSuccess);
        Assert.Equal("improved transcript", improved.Value);
    }

    [Fact]
    public async Task StartAsync_marks_job_failed_when_transcription_provider_fails()
    {
        using var temp = new TempDirectory();
        var options = Options.Create(new TranscriptionOptions { StorageRoot = temp.Path, MaxUploadBytes = 1000 });
        var store = new JsonTranscriptionStore(options);
        var storage = new LocalFileStorage(options, TimeProvider.System);
        var processor = new TranscriptionJobProcessor(
            options,
            store,
            storage,
            new FfmpegAudioProcessor(options),
            new FailingTranscriptionService(),
            new FakeTranscriptImprover("improved transcript"),
            TimeProvider.System);
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("fake audio"));

        var result = await processor.StartAsync(
            "sample.wav",
            "audio/wav",
            stream,
            stream.Length,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("openai.transcription_failed", result.Error!.Code);
    }

    private sealed class FakeTranscriptionService(string transcript) : ITranscriptionService
    {
        public Task<Result<string>> TranscribeAsync(PreparedAudio audio, CancellationToken cancellationToken) =>
            Task.FromResult(Result<string>.Success(transcript));
    }

    private sealed class FailingTranscriptionService : ITranscriptionService
    {
        public Task<Result<string>> TranscribeAsync(PreparedAudio audio, CancellationToken cancellationToken) =>
            Task.FromResult(Result<string>.Failure(TranscriptionError.Processing(
                "openai.transcription_failed",
                "The AI transcription service could not process the request. Try again later.")));
    }

    private sealed class FakeTranscriptImprover(string improvedTranscript) : ITranscriptImprover
    {
        public Task<Result<string>> ImproveAsync(string rawTranscript, CancellationToken cancellationToken) =>
            Task.FromResult(Result<string>.Success(improvedTranscript));
    }
}
