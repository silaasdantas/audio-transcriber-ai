namespace AudioTranscriberAI.Api.Infrastructure.Errors;

public static class SafeErrorMessages
{
    public const string MissingApiKey =
        "The transcription service is not configured. Set OPENAI_API_KEY and try again.";

    public const string FileSystemFailure =
        "The file could not be saved or read. Check local storage and try again.";

    public const string OpenAIFailure =
        "The AI transcription service could not process the request. Try again later.";

    public const string AudioProcessingFailure =
        "The audio file could not be prepared for transcription.";
}
