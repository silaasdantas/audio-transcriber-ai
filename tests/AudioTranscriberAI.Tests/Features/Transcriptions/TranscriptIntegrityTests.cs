using System.Text;
using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using AudioTranscriberAI.Api.Infrastructure.Audio;
using AudioTranscriberAI.Api.Infrastructure.Configuration;
using AudioTranscriberAI.Api.Infrastructure.Storage;
using Microsoft.Extensions.Options;

namespace AudioTranscriberAI.Tests.Features.Transcriptions;

public sealed class TranscriptIntegrityTests
{
    [Fact]
    public async Task Processor_persists_improved_transcript_that_preserves_names_numbers_claims_and_uncertainty()
    {
        using var temp = new TempDirectory();
        var options = Options.Create(new TranscriptionOptions { StorageRoot = temp.Path, MaxUploadBytes = 1000 });
        var store = new JsonTranscriptionStore(options);
        var storage = new LocalFileStorage(options, TimeProvider.System);
        var raw = "maria said invoice 42 was paid on friday but the account name was [unclear]";
        var improved = "Maria said invoice 42 was paid on Friday, but the account name was [unclear].";
        var processor = new TranscriptionJobProcessor(
            options,
            store,
            storage,
            new FfmpegAudioProcessor(options),
            new FakeTranscriptionService(raw),
            new FakeTranscriptImprover(improved),
            TimeProvider.System);
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("fake audio"));

        var result = await processor.StartAsync(
            "sample.mp3",
            "audio/mpeg",
            stream,
            stream.Length,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.ImprovedTranscriptPath);
        var saved = await storage.ReadTranscriptAsync(result.Value.Id, TranscriptKind.Improved, CancellationToken.None);
        Assert.True(saved.IsSuccess);
        Assert.Contains("Maria", saved.Value);
        Assert.Contains("42", saved.Value);
        Assert.Contains("paid on Friday", saved.Value);
        Assert.Contains("[unclear]", saved.Value);
    }

    private sealed class FakeTranscriptionService(string transcript) : ITranscriptionService
    {
        public Task<Result<string>> TranscribeAsync(PreparedAudio audio, CancellationToken cancellationToken) =>
            Task.FromResult(Result<string>.Success(transcript));
    }

    private sealed class FakeTranscriptImprover(string improvedTranscript) : ITranscriptImprover
    {
        public Task<Result<string>> ImproveAsync(string rawTranscript, CancellationToken cancellationToken) =>
            Task.FromResult(Result<string>.Success(improvedTranscript));
    }
}
