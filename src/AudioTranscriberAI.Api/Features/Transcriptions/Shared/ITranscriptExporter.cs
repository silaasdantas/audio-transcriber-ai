namespace AudioTranscriberAI.Api.Features.Transcriptions.Shared;

public interface ITranscriptExporter
{
    DownloadArtifact ExportMarkdown(
        string jobId,
        TranscriptKind type,
        string transcript);
}
