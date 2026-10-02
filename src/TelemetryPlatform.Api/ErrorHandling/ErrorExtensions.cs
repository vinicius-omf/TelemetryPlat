using Microsoft.AspNetCore.Http.HttpResults;
using TelemetryPlatform.Application.Common;

namespace TelemetryPlatform.Api.ErrorHandling;

internal static class ErrorExtensions
{
    public static ProblemHttpResult ToProblem(this Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };

        return TypedResults.Problem(statusCode: statusCode, title: error.Code, detail: error.Message);
    }
}
