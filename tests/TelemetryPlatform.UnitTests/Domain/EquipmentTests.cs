using Shouldly;
using TelemetryPlatform.Domain.Common;
using TelemetryPlatform.Domain.Equipments;

namespace TelemetryPlatform.UnitTests.Domain;

public sealed class EquipmentTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 2, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidData_NormalizesTagAndTrimsFields()
    {
        var equipment = Equipment.Create("  af1-tt-101 ", " Termopar cadinho ", " Alto Forno 1 ", Now);

        equipment.Tag.ShouldBe("AF1-TT-101");
        equipment.Name.ShouldBe("Termopar cadinho");
        equipment.Area.ShouldBe("Alto Forno 1");
        equipment.CreatedAt.ShouldBe(Now);
        equipment.Id.Version.ShouldBe(7);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("-AF1")]          // não pode começar com separador
    [InlineData("AF1 TT 101")]    // espaços internos
    [InlineData("AF1/TT/101")]    // caractere inválido
    public void Create_WithInvalidTag_Throws(string tag)
    {
        Should.Throw<DomainException>(() => Equipment.Create(tag, "Termopar", "Alto Forno 1", Now));
    }

    [Fact]
    public void Create_WithTagTooLong_Throws()
    {
        var tag = new string('A', Equipment.TagMaxLength + 1);

        Should.Throw<DomainException>(() => Equipment.Create(tag, "Termopar", "Alto Forno 1", Now));
    }

    [Theory]
    [InlineData("", "Alto Forno 1")]
    [InlineData("Termopar", " ")]
    public void Create_WithMissingNameOrArea_Throws(string name, string area)
    {
        Should.Throw<DomainException>(() => Equipment.Create("AF1-TT-101", name, area, Now));
    }
}
