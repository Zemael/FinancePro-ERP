namespace FinancePro.Application.Common.Results;

/// <summary>Resultado padronizado para casos de uso sem valor de retorno.</summary>
public class Result
{
    protected Result(bool isSuccess, string? message, IReadOnlyCollection<string> errors)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Message { get; }
    public IReadOnlyCollection<string> Errors { get; }

    public static Result Success(string? message = null) =>
        new(true, message, Array.Empty<string>());

    public static Result Ok(string? message = null) => Success(message);

    public static Result Failure(string error, string? message = null) =>
        new(false, message, new[] { error });

    public static Result Fail(string error, string? message = null) => Failure(error, message);

    public static Result Failure(IEnumerable<string> errors, string? message = null) =>
        new(false, message, errors.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray());

    public static Result Fail(IEnumerable<string> errors, string? message = null) => Failure(errors, message);
}
