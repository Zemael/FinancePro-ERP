namespace FinancePro.Application.Common.Results;

/// <summary>Resultado padronizado para casos de uso com valor de retorno.</summary>
public sealed class Result<T> : Result
{
    private Result(bool isSuccess, T? value, string? message, IReadOnlyCollection<string> errors)
        : base(isSuccess, message, errors)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value, string? message = null) =>
        new(true, value, message, Array.Empty<string>());

    public new static Result<T> Failure(string error, string? message = null) =>
        new(false, default, message, new[] { error });

    public new static Result<T> Failure(IEnumerable<string> errors, string? message = null) =>
        new(false, default, message, errors.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray());
}
