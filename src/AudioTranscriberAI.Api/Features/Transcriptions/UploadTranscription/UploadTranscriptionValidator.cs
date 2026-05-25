using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using AudioTranscriberAI.Api.Infrastructure.Configuration;

namespace AudioTranscriberAI.Api.Features.Transcriptions.UploadTranscription;

public static class UploadTranscriptionValidator
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3",
        ".wav",
        ".m4a"
    };

    public static Result<ValidatedUpload> Validate(
        string? fileName,
        long sizeBytes,
        TranscriptionOptions options)
    {
        if (sizeBytes <= 0)
        {
            return Result<ValidatedUpload>.Failure(TranscriptionError.Validation(
                "upload.empty_file",
                "Upload a non-empty MP3, WAV, or M4A file."));
        }

        if (sizeBytes > options.MaxUploadBytes)
        {
            return Result<ValidatedUpload>.Failure(TranscriptionError.Validation(
                "upload.file_too_large",
                $"The uploaded file exceeds the configured size limit of {options.MaxUploadBytes} bytes."));
        }

        var extension = Path.GetExtension(fileName ?? string.Empty);
        if (!SupportedExtensions.Contains(extension))
        {
            return Result<ValidatedUpload>.Failure(TranscriptionError.Validation(
                "upload.unsupported_format",
                "Only MP3, WAV, and M4A files are supported."));
        }

        var safeFileName = Path.GetFileName(fileName) ?? $"upload{extension}";
        return Result<ValidatedUpload>.Success(new ValidatedUpload(
            safeFileName,
            extension.TrimStart('.').ToLowerInvariant()));
    }
}

public sealed record ValidatedUpload(string FileName, string Extension);
