using Microsoft.Extensions.Time.Testing;
using Shouldly;
using TelemetryPlatform.Application.Common;
using TelemetryPlatform.Application.Equipments;

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
}
