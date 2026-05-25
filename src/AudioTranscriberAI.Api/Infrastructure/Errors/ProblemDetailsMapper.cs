using AudioTranscriberAI.Api.Features.Transcriptions.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AudioTranscriberAI.Api.Infrastructure.Errors;

public static class ProblemDetailsMapper
{
    public static IResult ToProblemResult(this TranscriptionError error, HttpContext httpContext)
    {
        var statusCode = error.Kind switch
        {
            TranscriptionErrorKind.Validation => StatusCodes.Status400BadRequest,
            TranscriptionErrorKind.NotFound => StatusCodes.Status404NotFound,
            TranscriptionErrorKind.Conflict => StatusCodes.Status409Conflict,
            TranscriptionErrorKind.Configuration => StatusCodes.Status503ServiceUnavailable,
            TranscriptionErrorKind.Processing => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = error.Code,
            Detail = error.Message,
            Status = statusCode,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        return Results.Problem(problem);
    }
}
