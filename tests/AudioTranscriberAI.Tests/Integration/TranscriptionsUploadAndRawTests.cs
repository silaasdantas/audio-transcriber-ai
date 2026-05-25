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

public sealed class TranscriptionsUploadAndRawTests
{
    [Fact]
    public async Task Upload_returns_accepted_and_raw_endpoint_returns_transcript()
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

        using var rawResponse = await client.GetAsync($"/api/transcriptions/{id}/raw");
        var raw = await rawResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Accepted, uploadResponse.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(id));
        Assert.Equal(HttpStatusCode.OK, rawResponse.StatusCode);
        Assert.Equal("raw transcript from fake service", raw);
        Assert.True(Directory.Exists(System.IO.Path.Combine(temp.Path, id!)));
    }

    [Fact]
    public async Task Upload_rejects_unsupported_format()
    {
        using var temp = new TempDirectory();
        await using var factory = new TestApiFactory(temp.Path);
        using var client = factory.CreateClient();
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("not audio")), "file", "sample.txt");

        using var response = await client.PostAsync("/api/transcriptions", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
                services.AddSingleton<ITranscriptionService>(_ => new FakeTranscriptionService());
            });
        }
    }

    private sealed class FakeTranscriptionService : ITranscriptionService
    {
        public Task<Result<string>> TranscribeAsync(PreparedAudio audio, CancellationToken cancellationToken) =>
            Task.FromResult(Result<string>.Success("raw transcript from fake service"));
    }
}
