using Microsoft.AspNetCore.Http.HttpResults;
using TelemetryPlatform.Api.ErrorHandling;
using TelemetryPlatform.Application.Equipments;

namespace TelemetryPlatform.Api.Endpoints;

/// <summary>
/// Minimal APIs agrupadas por recurso. TypedResults deixa os status possíveis explícitos
/// na assinatura — e o OpenAPI é gerado a partir deles.
/// </summary>
internal static class EquipmentEndpoints
{
    private const string GetByIdRoute = "GetEquipmentById";

    public static IEndpointRouteBuilder MapEquipmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/equipments").WithTags("Equipments");

        group.MapPost("/", CreateAsync);
        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", GetByIdAsync).WithName(GetByIdRoute);
        group.MapDelete("/{id:guid}", DeleteAsync);
        group.MapPut("/{id:guid}", UpdateAsync);

        return app;
    }

    private static async Task<Results<CreatedAtRoute<EquipmentResponse>, ProblemHttpResult>> CreateAsync(
        CreateEquipmentRequest request, EquipmentService service, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, GetByIdRoute, new { id = result.Value.Id })
            : result.Error.ToProblem();
    }

    private static async Task<Ok<IReadOnlyList<EquipmentResponse>>> ListAsync(
        EquipmentService service, CancellationToken cancellationToken) =>
        TypedResults.Ok(await service.ListAsync(cancellationToken));

    private static async Task<Results<Ok<EquipmentResponse>, ProblemHttpResult>> GetByIdAsync(
        Guid id, EquipmentService service, CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteAsync(
        Guid id, EquipmentService service, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);

        return result.IsSuccess ? TypedResults.NoContent() : result.Error.ToProblem();
    }

    private static async Task<Results<Ok<EquipmentResponse>, ProblemHttpResult>> UpdateAsync(
        Guid id, UpdateEquipmentRequest request, EquipmentService service, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();
    }
}
