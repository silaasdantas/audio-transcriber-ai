using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using AudioTranscriberAI.Api.Infrastructure.Errors;

namespace AudioTranscriberAI.Api.Features.Transcriptions.GetImprovedTranscript;

public static class GetImprovedTranscriptEndpoint
{
    public static IEndpointRouteBuilder MapGetImprovedTranscript(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/transcriptions/{id}/improved", async (
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

                if (job.Value!.ImprovedTranscriptPath is null)
                {
                    return TranscriptionError.Conflict(
                            "transcript.not_ready",
                            "The improved transcript is not available yet.")
                        .ToProblemResult(httpContext);
                }

                var improved = await storage.ReadTranscriptAsync(id, TranscriptKind.Improved, cancellationToken);
                return improved.IsSuccess
                    ? Results.Text(improved.Value!, "text/plain")
                    : improved.Error!.ToProblemResult(httpContext);
            })
            .WithName("GetImprovedTranscript")
            .WithTags("Transcriptions")
            .Produces<string>(StatusCodes.Status200OK, "text/plain")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithOpenApi();

        return endpoints;
    }
}
