using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using AudioTranscriberAI.Api.Infrastructure.Errors;

namespace AudioTranscriberAI.Api.Features.Transcriptions.GetRawTranscript;

public static class GetRawTranscriptEndpoint
{
    public static IEndpointRouteBuilder MapGetRawTranscript(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/transcriptions/{id}/raw", async (
                string id,
                ITranscriptionStore store,
                ILocalFileStorage storage,
                HttpContext httpContext,
                CancellationToken cancellationToken) =>
            {
                var job = await store.GetAsync(id, cancellationToken);
                if (!job.IsSuccess)
                {
                    return job.Error!.ToProblemResult(httpContext);
                }

                if (job.Value!.RawTranscriptPath is null)
                {
                    return TranscriptionError.Conflict(
                            "transcript.not_ready",
                            "The raw transcript is not available yet.")
                        .ToProblemResult(httpContext);
                }

                var raw = await storage.ReadTranscriptAsync(id, TranscriptKind.Raw, cancellationToken);
                return raw.IsSuccess
                    ? Results.Text(raw.Value!, "text/plain")
                    : raw.Error!.ToProblemResult(httpContext);
            })
            .WithName("GetRawTranscript")
            .WithTags("Transcriptions")
            .Produces<string>(StatusCodes.Status200OK, "text/plain")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithOpenApi();

        return endpoints;
    }
}
