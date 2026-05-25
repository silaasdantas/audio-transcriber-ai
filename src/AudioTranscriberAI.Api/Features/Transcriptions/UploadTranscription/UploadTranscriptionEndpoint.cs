using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using AudioTranscriberAI.Api.Infrastructure.Errors;

namespace AudioTranscriberAI.Api.Features.Transcriptions.UploadTranscription;

public static class UploadTranscriptionEndpoint
{
    public static IEndpointRouteBuilder MapUploadTranscription(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/transcriptions", async (
                IFormFile? file,
                ITranscriptionJobProcessor processor,
                HttpContext httpContext,
                CancellationToken cancellationToken) =>
            {
                if (file is null)
                {
                    return TranscriptionError.Validation(
                            "upload.file_required",
                            "Upload an MP3, WAV, or M4A file.")
                        .ToProblemResult(httpContext);
                }

                var result = await processor.StartAsync(
                    file.FileName,
                    file.ContentType,
                    file.OpenReadStream(),
                    file.Length,
                    cancellationToken);

                if (!result.IsSuccess)
                {
                    return result.Error!.ToProblemResult(httpContext);
                }

                var statusUrl = $"/api/transcriptions/{result.Value!.Id}";
                return Results.Accepted(statusUrl, new UploadTranscriptionResponse(
                    result.Value.Id,
                    result.Value.Status.ToString(),
                    statusUrl));
            })
            .WithName("UploadTranscription")
            .WithTags("Transcriptions")
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<UploadTranscriptionResponse>(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status502BadGateway)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
            .WithOpenApi();

        return endpoints;
    }
}

public sealed record UploadTranscriptionResponse(
    string Id,
    string Status,
    string StatusUrl);
