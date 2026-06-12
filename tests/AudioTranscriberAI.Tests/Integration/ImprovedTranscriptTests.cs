using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AudioTranscriberAI.Tests.Integration;

public sealed class ImprovedTranscriptTests
{
    [Fact]
    public async Task Upload_processes_improvement_and_improved_endpoint_returns_text()
    {
        using var temp = new TempDirectory();
        await using var factory = new TestApiFactory(temp.Path);
        using var client = factory.CreateClient();
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes("fake audio"));
        file.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
        form.Add(file, "file", "sample.mp3");

        using var uploadResponse = await client.PostAsync("/api/transcriptions", form);
        var uploadBody = await uploadResponse.Content.ReadAsStringAsync();
        using var uploadJson = JsonDocument.Parse(uploadBody);
        var id = uploadJson.RootElement.GetProperty("id").GetString();

        using var improvedResponse = await client.GetAsync($"/api/transcriptions/{id}/improved");
        var improved = await improvedResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Accepted, uploadResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, improvedResponse.StatusCode);
        Assert.Equal("Improved transcript from fake service.", improved);
    }

    private sealed class TestApiFactory(string storageRoot) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration(configuration =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Transcriptions:StorageRoot"] = storageRoot,
                    ["Transcriptions:MaxUploadBytes"] = "1048576",
                    ["Transcriptions:FfmpegPath"] = "ffmpeg",
                    ["Transcriptions:DownloadFilePrefix"] = "transcript",
                    ["OpenAI:TranscriptionModel"] = "test-transcription",
                    ["OpenAI:TextModel"] = "test-text"
                });
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ITranscriptionService>();
                services.RemoveAll<ITranscriptImprover>();
                services.AddSingleton<ITranscriptionService>(_ => new FakeTranscriptionService());
                services.AddSingleton<ITranscriptImprover>(_ => new FakeTranscriptImprover());
            });
        }
    }

    private sealed class FakeTranscriptionService : ITranscriptionService
    {
        public Task<Result<string>> TranscribeAsync(PreparedAudio audio, CancellationToken cancellationToken) =>
            Task.FromResult(Result<string>.Success("raw transcript from fake service"));
    }

    private sealed class FakeTranscriptImprover : ITranscriptImprover
    {
        public Task<Result<string>> ImproveAsync(string rawTranscript, CancellationToken cancellationToken) =>
            Task.FromResult(Result<string>.Success("Improved transcript from fake service."));
    }
}
