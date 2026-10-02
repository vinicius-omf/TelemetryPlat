using System.Text.RegularExpressions;
using TelemetryPlatform.Domain.Common;

namespace TelemetryPlatform.Domain.Equipments;

/// <summary>
/// Equipamento monitorado na planta (ex.: um termopar do Alto Forno 1, tag "AF1-TT-101").
/// Entidade rica: o estado só muda através de métodos que garantem as invariantes,
/// por isso os setters são privados e a criação passa por <see cref="Create"/>.
/// </summary>
public sealed partial class Equipment
{
    public const int TagMaxLength = 50;
    public const int NameMaxLength = 200;
    public const int AreaMaxLength = 100;

    public Guid Id { get; private set; }
    public string Tag { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Area { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    // Construtor usado pelo EF Core para materializar a entidade a partir do banco.
    private Equipment() { }

    public static Equipment Create(string tag, string name, string area, DateTimeOffset createdAt)
    {
        var normalizedTag = tag?.Trim().ToUpperInvariant() ?? string.Empty;

        if (normalizedTag.Length is 0 or > TagMaxLength || !TagPattern().IsMatch(normalizedTag))
        {
            throw new DomainException(
                $"Tag inválida: use de 1 a {TagMaxLength} caracteres (letras, números, '-', '_' ou '.').");
        }

        // .NET tem precisão de 100ns, PostgreSQL de 1µs: truncamos para que o valor retornado
        // na criação seja idêntico ao lido do banco depois.
        createdAt = createdAt.AddTicks(-(createdAt.Ticks % TimeSpan.TicksPerMicrosecond));

        return new Equipment
        {
            // Guid v7 é ordenado por tempo: melhor para índices do banco que Guid.NewGuid().
            Id = Guid.CreateVersion7(createdAt),
            Tag = normalizedTag,
            Name = Required(name, nameof(Name), NameMaxLength),
            Area = Required(area, nameof(Area), AreaMaxLength),
            CreatedAt = createdAt,
        };
    }

    private static string Required(string value, string field, int maxLength)
    {
        var trimmed = value?.Trim() ?? string.Empty;

        if (trimmed.Length == 0 || trimmed.Length > maxLength)
        {
            throw new DomainException($"{field} é obrigatório e deve ter no máximo {maxLength} caracteres.");
        }

        return trimmed;
    }

    // Regex gerado em tempo de compilação (source generator): mais rápido e sem custo de inicialização.
    [GeneratedRegex("^[A-Z0-9][A-Z0-9._-]*$")]
    private static partial Regex TagPattern();
}
