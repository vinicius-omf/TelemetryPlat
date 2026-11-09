using TelemetryPlatform.Application.Equipments;
using TelemetryPlatform.Domain.Equipments;

namespace TelemetryPlatform.UnitTests.Application;

/// <summary>
/// Fake escrito à mão: para repositórios simples costuma ser mais legível que um mock.
/// </summary>
internal sealed class InMemoryEquipmentRepository : IEquipmentRepository
{
    private readonly List<Equipment> _pending = [];
    private readonly List<Equipment> _pendingToRemove = [];
    public int SaveChangesCount { get; private set; }
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
        SaveChangesCount++;
        Saved.AddRange(_pending);
        _pending.Clear();

        Saved.RemoveAll(e => _pendingToRemove.Contains(e));
        _pendingToRemove.Clear();
        
        return Task.CompletedTask;
    }

    public void Delete(Equipment equipment)
    {
        // Removido antes de salvar: o EF desfaz o Add e não envia nada ao banco.
        if (_pending.Remove(equipment))
        {
            return;
        }

        if (!Saved.Contains(equipment))
        {
            throw new InvalidOperationException("Tentativa de remover equipamento que não está salvo.");
        }

        _pendingToRemove.Add(equipment);
    }
}
