using AudioTranscriberAI.Api.Features.Transcriptions.UploadTranscription;
using AudioTranscriberAI.Api.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AudioTranscriberAI.Api.Features.Transcriptions.Shared;

public sealed class TranscriptionJobProcessor(
    IOptions<TranscriptionOptions> options,
    ITranscriptionStore store,
    ILocalFileStorage storage,
    IAudioProcessor audioProcessor,
    ITranscriptionService transcriptionService,
    ITranscriptImprover transcriptImprover,
    TimeProvider timeProvider) : ITranscriptionJobProcessor
{
    private readonly TranscriptionOptions _options = options.Value;

    public async Task<Result<TranscriptionJob>> StartAsync(
        string originalFileName,
        string? contentType,
        Stream content,
        long sizeBytes,
        CancellationToken cancellationToken)
    {
        var validation = UploadTranscriptionValidator.Validate(originalFileName, sizeBytes, _options);
        if (!validation.IsSuccess)
        {
            return Result<TranscriptionJob>.Failure(validation.Error!);
        }

        var now = timeProvider.GetUtcNow();
        var id = Guid.NewGuid().ToString("N");
        var upload = await storage.SaveOriginalAsync(
            id,
            validation.Value!.FileName,
            contentType,
            content,
            sizeBytes,
            cancellationToken);

        if (!upload.IsSuccess)
        {
            return Result<TranscriptionJob>.Failure(upload.Error!);
        }

        var job = new TranscriptionJob(
            id,
            upload.Value!.OriginalFileName,
            Path.GetFileName(upload.Value.StoredPath),
            upload.Value.Extension,
            upload.Value.SizeBytes,
            TranscriptionStatus.Pending,
            now,
            now);

        await store.SaveAsync(job, cancellationToken);

        job = await UpdateStatusAsync(id, TranscriptionStatus.Processing, cancellationToken);

        var preparedAudio = await audioProcessor.PrepareAsync(upload.Value, cancellationToken);
        if (!preparedAudio.IsSuccess)
        {
            return await FailAsync(id, preparedAudio.Error!, cancellationToken);
        }

        var transcript = await transcriptionService.TranscribeAsync(preparedAudio.Value!, cancellationToken);
        if (!transcript.IsSuccess)
        {
            return await FailAsync(id, transcript.Error!, cancellationToken);
        }

        var rawArtifact = await storage.SaveTranscriptAsync(
            id,
            TranscriptKind.Raw,
            transcript.Value!,
            cancellationToken);

        if (!rawArtifact.IsSuccess)
        {
            return await FailAsync(id, rawArtifact.Error!, cancellationToken);
        }

        var improvedTranscript = await transcriptImprover.ImproveAsync(transcript.Value!, cancellationToken);
        if (!improvedTranscript.IsSuccess)
        {
            return await FailAsync(id, improvedTranscript.Error!, cancellationToken);
        }

        var improvedArtifact = await storage.SaveTranscriptAsync(
            id,
            TranscriptKind.Improved,
            improvedTranscript.Value!,
            cancellationToken);

        if (!improvedArtifact.IsSuccess)
        {
            return await FailAsync(id, improvedArtifact.Error!, cancellationToken);
        }

        var completed = await store.UpdateAsync(
            id,
            current => current with
            {
                Status = TranscriptionStatus.Completed,
                UpdatedAtUtc = timeProvider.GetUtcNow(),
                RawTranscriptPath = rawArtifact.Value!.Path,
                ImprovedTranscriptPath = improvedArtifact.Value!.Path
            },
            cancellationToken);

        return completed;
    }

    private async Task<TranscriptionJob> UpdateStatusAsync(
        string id,
        TranscriptionStatus status,
        CancellationToken cancellationToken)
    {
        var result = await store.UpdateAsync(
            id,
            current => current with { Status = status, UpdatedAtUtc = timeProvider.GetUtcNow() },
            cancellationToken);

        return result.Value!;
    }

    private async Task<Result<TranscriptionJob>> FailAsync(
        string id,
        TranscriptionError error,
        CancellationToken cancellationToken)
    {
        await store.UpdateAsync(
            id,
            current => current with
            {
                Status = TranscriptionStatus.Failed,
                UpdatedAtUtc = timeProvider.GetUtcNow(),
                FailureCode = error.Code,
                FailureMessage = error.Message
            },
            cancellationToken);

        return Result<TranscriptionJob>.Failure(error);
    }
}
