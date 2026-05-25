namespace AudioTranscriberAI.Api.Infrastructure.Configuration;

public sealed class OpenAIOptions
{
    public const string SectionName = "OpenAI";

    public string? ApiKey { get; init; }

    public string TranscriptionModel { get; init; } = "whisper-1";

    public string TextModel { get; init; } = "gpt-4.1-mini";

    public bool HasApiKey => !string.IsNullOrWhiteSpace(ApiKey);
}
