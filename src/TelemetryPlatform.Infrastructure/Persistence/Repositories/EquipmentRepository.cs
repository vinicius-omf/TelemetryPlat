using Microsoft.EntityFrameworkCore;
using TelemetryPlatform.Application.Equipments;
using TelemetryPlatform.Domain.Equipments;

namespace TelemetryPlatform.Infrastructure.Persistence.Repositories;

internal sealed class EquipmentRepository(TelemetryDbContext dbContext) : IEquipmentRepository
{
    public Task<bool> TagExistsAsync(string tag, CancellationToken cancellationToken) =>
        dbContext.Equipments.AnyAsync(e => e.Tag == tag, cancellationToken);

    public Task<Equipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Equipments.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Equipment>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Equipments.AsNoTracking().OrderBy(e => e.Tag).ToListAsync(cancellationToken);

    public void Add(Equipment equipment) => dbContext.Equipments.Add(equipment);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
