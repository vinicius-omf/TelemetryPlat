using TelemetryPlatform.Application.Equipments;
using TelemetryPlatform.Domain.Equipments;

namespace TelemetryPlatform.UnitTests.Application;

/// <summary>
/// Fake escrito à mão: para repositórios simples costuma ser mais legível que um mock.
/// </summary>
internal sealed class InMemoryEquipmentRepository : IEquipmentRepository
{
    private readonly List<Equipment> _pending = [];

    public List<Equipment> Saved { get; } = [];

    public Task<bool> TagExistsAsync(string tag, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Any(e => e.Tag == tag));

    public Task<Equipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.FirstOrDefault(e => e.Id == id));

    public Task<IReadOnlyList<Equipment>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Equipment>>([.. Saved.OrderBy(e => e.Tag)]);

    public void Add(Equipment equipment) => _pending.Add(equipment);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        return Task.CompletedTask;
    }
}
