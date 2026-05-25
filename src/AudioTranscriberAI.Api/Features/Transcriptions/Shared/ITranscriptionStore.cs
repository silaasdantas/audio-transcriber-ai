namespace AudioTranscriberAI.Api.Features.Transcriptions.Shared;

public interface ITranscriptionStore
{
    Task SaveAsync(TranscriptionJob job, CancellationToken cancellationToken);

    Task<Result<TranscriptionJob>> GetAsync(string id, CancellationToken cancellationToken);

    Task<Result<TranscriptionJob>> UpdateAsync(
        string id,
        Func<TranscriptionJob, TranscriptionJob> update,
        CancellationToken cancellationToken);
}
