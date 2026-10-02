using Microsoft.AspNetCore.Diagnostics;
using TelemetryPlatform.Domain.Common;

namespace TelemetryPlatform.Api.ErrorHandling;

/// <summary>
/// Converte violações de regra de domínio em 400 Bad Request no formato ProblemDetails (RFC 9457).
/// Outras exceções seguem para o handler padrão (500).
/// </summary>
internal sealed class DomainExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DomainException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Requisição inválida",
                Detail = exception.Message,
            },
        });
    }
}
