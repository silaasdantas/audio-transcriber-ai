using System.Net.Http.Headers;
using System.Text.Json;
using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using AudioTranscriberAI.Api.Infrastructure.Configuration;
using AudioTranscriberAI.Api.Infrastructure.Errors;
using Microsoft.Extensions.Options;

namespace AudioTranscriberAI.Api.Infrastructure.OpenAI;

public sealed class OpenAITranscriptionService(
    HttpClient httpClient,
    IOptions<OpenAIOptions> options,
    ILogger<OpenAITranscriptionService> logger) : ITranscriptionService
{
    private readonly OpenAIOptions _options = options.Value;

    public async Task<Result<string>> TranscribeAsync(
        PreparedAudio audio,
        CancellationToken cancellationToken)
    {
        if (!_options.HasApiKey)
        {
            return Result<string>.Failure(TranscriptionError.Configuration(SafeErrorMessages.MissingApiKey));
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/audio/transcriptions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            await using var audioStream = File.OpenRead(audio.Path);
            using var form = new MultipartFormDataContent
            {
                { new StringContent(_options.TranscriptionModel), "model" },
                { new StreamContent(audioStream), "file", Path.GetFileName(audio.Path) }
            };
            request.Content = form;

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "OpenAI transcription failed for job {JobId} with status {StatusCode}",
                    audio.JobId,
                    response.StatusCode);

                return Result<string>.Failure(TranscriptionError.Processing(
                    "openai.transcription_failed",
                    SafeErrorMessages.OpenAIFailure));
            }

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
            var transcript = json.RootElement.TryGetProperty("text", out var text)
                ? text.GetString()
                : null;

            return string.IsNullOrWhiteSpace(transcript)
                ? Result<string>.Failure(TranscriptionError.Processing(
                    "openai.empty_transcript",
                    "The transcription service returned an empty transcript."))
                : Result<string>.Success(transcript);
        }
        catch (Exception ex) when (ex is IOException or HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(
                ex,
                "OpenAI transcription request failed for job {JobId}",
                audio.JobId);

            return Result<string>.Failure(TranscriptionError.Processing(
                "openai.transcription_failed",
                SafeErrorMessages.OpenAIFailure));
        }
    }
}
