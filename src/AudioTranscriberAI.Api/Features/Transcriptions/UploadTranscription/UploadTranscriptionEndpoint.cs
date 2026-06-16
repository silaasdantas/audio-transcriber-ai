using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using AudioTranscriberAI.Api.Infrastructure.Errors;
using Microsoft.OpenApi.Models;

namespace AudioTranscriberAI.Api.Features.Transcriptions.UploadTranscription;

public static class UploadTranscriptionEndpoint
{
    public static IEndpointRouteBuilder MapUploadTranscription(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/transcriptions", async (
                HttpRequest request,
                ITranscriptionJobProcessor processor,
                HttpContext httpContext,
                CancellationToken cancellationToken) =>
            {
                var file = await GetUploadedFileAsync(request, cancellationToken);
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
            .Accepts<UploadTranscriptionForm>("multipart/form-data")
            .Produces<UploadTranscriptionResponse>(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status502BadGateway)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
            .WithOpenApi(operation =>
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Content =
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Required = new HashSet<string> { "file" },
                                Properties =
                                {
                                    ["file"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary",
                                        Description = "MP3, WAV, or M4A audio file."
                                    }
                                }
                            }
                        }
                    }
                };

                return operation;
            });

        return endpoints;
    }

    private static async Task<IFormFile?> GetUploadedFileAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
        {
            return null;
        }

        var form = await request.ReadFormAsync(cancellationToken);
        return form.Files.GetFile("file");
    }
}

public sealed record UploadTranscriptionResponse(
    string Id,
    string Status,
    string StatusUrl);

public sealed class UploadTranscriptionForm
{
    public IFormFile File { get; init; } = null!;
}
