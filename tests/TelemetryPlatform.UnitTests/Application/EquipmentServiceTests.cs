using Microsoft.Extensions.Time.Testing;
using Shouldly;
using TelemetryPlatform.Application.Common;
using TelemetryPlatform.Application.Equipments;
using TelemetryPlatform.Domain.Common;

namespace TelemetryPlatform.UnitTests.Application;

public sealed class EquipmentServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 2, 12, 0, 0, TimeSpan.Zero);

    private readonly InMemoryEquipmentRepository _repository = new();
    private readonly EquipmentService _service;

    public EquipmentServiceTests()
    {
        _service = new EquipmentService(_repository, new FakeTimeProvider(Now));
    }

    [Fact]
    public async Task CreateAsync_WithNewTag_SavesAndReturnsEquipment()
    {
        var result = await _service.CreateAsync(
            new CreateEquipmentRequest("af1-tt-101", "Termopar cadinho", "Alto Forno 1"),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Tag.ShouldBe("AF1-TT-101");
        result.Value.CreatedAt.ShouldBe(Now);
        _repository.Saved.ShouldHaveSingleItem().Id.ShouldBe(result.Value.Id);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateTag_ReturnsConflict()
    {
        var request = new CreateEquipmentRequest("AF1-TT-101", "Termopar cadinho", "Alto Forno 1");
        await _service.CreateAsync(request, TestContext.Current.CancellationToken);

        // Mesma tag com outra caixa: deve ser considerada duplicada após a normalização.
        var result = await _service.CreateAsync(
            request with { Tag = "af1-tt-101" }, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error.Type.ShouldBe(ErrorType.Conflict);
        _repository.Saved.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNotFound()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error.Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_RemovesEquipment()
    {
        var createResult = await _service.CreateAsync(
            new CreateEquipmentRequest("af1-tt-101", "Termopar cadinho", "Alto Forno 1"),
            TestContext.Current.CancellationToken);

        _repository.Saved.ShouldHaveSingleItem();

        createResult.ShouldNotBeNull();
        createResult.Value.ShouldNotBeNull();
        var equipmentId = createResult.Value.Id;

        var deleteResult = await _service.DeleteAsync(equipmentId, TestContext.Current.CancellationToken);
        deleteResult.IsSuccess.ShouldBeTrue();
        _repository.Saved.ShouldBeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_WhenEquipmentDoesNotExist_ReturnsNotFound()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error.Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_WhenExists_UpdatesNameAndArea()
    {
        var createResult = await _service.CreateAsync(
            new CreateEquipmentRequest("af1-tt-101", "Termopar cadinho", "Alto Forno 1"),
            TestContext.Current.CancellationToken);

        createResult.IsSuccess.ShouldBeTrue();

        var equipmentId = createResult.Value.Id;
        var updateResult = await _service.UpdateAsync(
            equipmentId,
            new UpdateEquipmentRequest("Termopar atualizado", "Alto Forno 2"),
            TestContext.Current.CancellationToken);

        updateResult.IsSuccess.ShouldBeTrue();

        var updatedEquipment = await _service.GetByIdAsync(equipmentId, TestContext.Current.CancellationToken);
        updatedEquipment.IsSuccess.ShouldBeTrue();
        updatedEquipment.Value.Name.ShouldBe("Termopar atualizado");
        updatedEquipment.Value.Area.ShouldBe("Alto Forno 2");
        updatedEquipment.Value.Tag.ShouldBe(createResult.Value.Tag);
        updatedEquipment.Value.Id.ShouldBe(equipmentId);
        
        _repository.SaveChangesCount.ShouldBe(2); // Create + Update
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_ReturnsNotFound()
    {
        var result = await _service.UpdateAsync(
            Guid.NewGuid(),
            new UpdateEquipmentRequest("Termopar atualizado", "Alto Forno 2"),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error.Type.ShouldBe(ErrorType.NotFound);
    }

    [Theory]
    [InlineData("", "Alto Forno 2")]
    [InlineData("Termopar", "   ")]
    public async Task Update_WithMissingNameOrArea_Throws(string name, string area)
    {
        
        var createdResult = await _service.CreateAsync(
            new CreateEquipmentRequest("af1-tt-101", "Termopar cadinho", "Alto Forno 1"),
            TestContext.Current.CancellationToken);
        createdResult.IsSuccess.ShouldBeTrue();
        createdResult.Value.ShouldNotBeNull();
        var equipmentId = createdResult.Value.Id;

        var equipment = await _service.GetByIdAsync(equipmentId, TestContext.Current.CancellationToken);
        equipment.IsSuccess.ShouldBeTrue();
        equipment.Value.ShouldNotBeNull();

        await Should.ThrowAsync<DomainException>(() => _service.UpdateAsync(
            equipmentId,
            new UpdateEquipmentRequest(name, area),
            TestContext.Current.CancellationToken));
    }
}
  
  
    