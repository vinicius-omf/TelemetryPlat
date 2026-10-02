using TelemetryPlatform.Application.Common;
using TelemetryPlatform.Domain.Equipments;

namespace TelemetryPlatform.Application.Equipments;

public sealed record CreateEquipmentRequest(string Tag, string Name, string Area);

public sealed record EquipmentResponse(Guid Id, string Tag, string Name, string Area, DateTimeOffset CreatedAt)
{
    public static EquipmentResponse From(Equipment equipment) =>
        new(equipment.Id, equipment.Tag, equipment.Name, equipment.Area, equipment.CreatedAt);
}

public static class EquipmentErrors
{
    public static Error TagAlreadyExists(string tag) =>
        Error.Conflict("Equipment.TagAlreadyExists", $"Já existe um equipamento com a tag '{tag}'.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("Equipment.NotFound", $"Equipamento '{id}' não encontrado.");
}
