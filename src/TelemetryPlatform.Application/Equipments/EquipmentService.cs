using TelemetryPlatform.Application.Common;
using TelemetryPlatform.Domain.Equipments;

namespace TelemetryPlatform.Application.Equipments;

// TimeProvider (.NET 8+) substitui DateTime.Now: nos testes usamos um FakeTimeProvider com hora controlada.
public sealed class EquipmentService(IEquipmentRepository repository, TimeProvider timeProvider)
{
    public async Task<Result<EquipmentResponse>> CreateAsync(
        CreateEquipmentRequest request, CancellationToken cancellationToken)
    {
        // Create valida e normaliza a tag; a checagem de duplicidade usa a tag já normalizada.
        var equipment = Equipment.Create(request.Tag, request.Name, request.Area, timeProvider.GetUtcNow());

        if (await repository.TagExistsAsync(equipment.Tag, cancellationToken))
        {
            return EquipmentErrors.TagAlreadyExists(equipment.Tag);
        }

        repository.Add(equipment);
        await repository.SaveChangesAsync(cancellationToken);

        return EquipmentResponse.From(equipment);
    }

    public async Task<Result<EquipmentResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var equipment = await repository.GetByIdAsync(id, cancellationToken);

        return equipment is null
            ? EquipmentErrors.NotFound(id)
            : EquipmentResponse.From(equipment);
    }

    public async Task<IReadOnlyList<EquipmentResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var equipments = await repository.ListAsync(cancellationToken);
        return [.. equipments.Select(EquipmentResponse.From)];
    }
    
    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var equipment = await repository.GetByIdAsync(id, cancellationToken);

        if (equipment is null)
        {
            return EquipmentErrors.NotFound(id);
        }
        repository.Delete(equipment);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<Result<EquipmentResponse>> UpdateAsync(Guid id, UpdateEquipmentRequest request, CancellationToken cancellationToken)
    {
        var equipment = await repository.GetByIdAsync(id, cancellationToken);
        if (equipment is null)
        {
            return EquipmentErrors.NotFound(id);
        }

        equipment.Update(request.Name, request.Area);
        await repository.SaveChangesAsync(cancellationToken);

        return EquipmentResponse.From(equipment); 
    }
}
