namespace AudioTranscriberAI.Api.Features.Transcriptions.Shared;

public sealed record TranscriptionError(
    string Code,
    string Message,
    TranscriptionErrorKind Kind)
{
    public static TranscriptionError Validation(string code, string message) =>
        new(code, message, TranscriptionErrorKind.Validation);

    public static TranscriptionError NotFound(string message = "Transcription was not found.") =>
        new("transcription.not_found", message, TranscriptionErrorKind.NotFound);

    public static TranscriptionError Conflict(string code, string message) =>
        new(code, message, TranscriptionErrorKind.Conflict);

    public static TranscriptionError Configuration(string message) =>
        new("configuration.missing", message, TranscriptionErrorKind.Configuration);

    public static TranscriptionError Processing(string code, string message) =>
        new(code, message, TranscriptionErrorKind.Processing);
}

public enum TranscriptionErrorKind
{
    Validation,
    NotFound,
    Conflict,
    Configuration,
    Processing,
    Unexpected
}

public readonly record struct Result<T>(T? Value, TranscriptionError? Error)
{
    public bool IsSuccess => Error is null;

    public static Result<T> Success(T value) => new(value, null);

    public static Result<T> Failure(TranscriptionError error) => new(default, error);
}
