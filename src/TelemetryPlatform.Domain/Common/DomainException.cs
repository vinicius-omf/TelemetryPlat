namespace TelemetryPlatform.Domain.Common;

/// <summary>
/// Lançada quando uma regra de negócio (invariante) do domínio é violada.
/// A API converte essa exceção em HTTP 400 (veja DomainExceptionHandler).
/// </summary>
public sealed class DomainException(string message) : Exception(message);
