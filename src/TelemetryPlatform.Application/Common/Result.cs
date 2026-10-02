using System.Diagnostics.CodeAnalysis;

namespace TelemetryPlatform.Application.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
}

public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
}

/// <summary>
/// Result pattern: falhas de negócio esperadas (não encontrado, duplicado...) são retornadas
/// como valor em vez de exceção. Exceções ficam para o que é realmente excepcional.
/// </summary>
public sealed class Result<T>
{
    private Result(T? value, Error? error)
    {
        Value = value;
        Error = error;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    public T? Value { get; }
    public Error? Error { get; }

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(Error error) => new(default, error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}
