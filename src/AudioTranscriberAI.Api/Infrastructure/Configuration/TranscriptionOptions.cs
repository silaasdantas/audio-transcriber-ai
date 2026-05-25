namespace AudioTranscriberAI.Api.Infrastructure.Configuration;

public sealed class TranscriptionOptions
{
    public const string SectionName = "Transcriptions";

    public string StorageRoot { get; init; } = "data/transcriptions";

    public long MaxUploadBytes { get; init; } = 104_857_600;

    public string FfmpegPath { get; init; } = "ffmpeg";

    public string DownloadFilePrefix { get; init; } = "transcript";
}
