using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using AudioTranscriberAI.Api.Infrastructure.Configuration;
using AudioTranscriberAI.Api.Infrastructure.Errors;
using Microsoft.Extensions.Options;

namespace AudioTranscriberAI.Api.Infrastructure.OpenAI;

public sealed class OpenAITranscriptImprover(
    HttpClient httpClient,
    IOptions<OpenAIOptions> options,
    ILogger<OpenAITranscriptImprover> logger) : ITranscriptImprover
{
    private readonly OpenAIOptions _options = options.Value;

    public async Task<Result<string>> ImproveAsync(
        string rawTranscript,
        CancellationToken cancellationToken)
    {
        if (!_options.HasApiKey)
        {
            return Result<string>.Failure(TranscriptionError.Configuration(SafeErrorMessages.MissingApiKey));
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            var payload = new
            {
                model = _options.TextModel,
                temperature = 0,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = TranscriptImprovementPromptBuilder.Build(rawTranscript)
                    }
                }
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "OpenAI transcript improvement failed with status {StatusCode}",
                    response.StatusCode);

                return Result<string>.Failure(TranscriptionError.Processing(
                    "openai.improvement_failed",
                    SafeErrorMessages.OpenAIFailure));
            }

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
            var improved = json.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return string.IsNullOrWhiteSpace(improved)
                ? Result<string>.Failure(TranscriptionError.Processing(
                    "openai.empty_improvement",
                    "The transcript improvement service returned an empty transcript."))
                : Result<string>.Success(improved.Trim());
        }
        catch (Exception ex) when (ex is IOException or HttpRequestException or JsonException or TaskCanceledException)
        {
            logger.LogWarning(ex, "OpenAI transcript improvement request failed");

            return Result<string>.Failure(TranscriptionError.Processing(
                "openai.improvement_failed",
                SafeErrorMessages.OpenAIFailure));
        }
    }
}
