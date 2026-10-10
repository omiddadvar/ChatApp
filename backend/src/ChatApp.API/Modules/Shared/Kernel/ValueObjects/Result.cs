using System.Text.Json.Serialization;

namespace ChatApp.API.Modules.Shared.Kernel.ValueObjects;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    [JsonConstructor]
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Successful result cannot carry an error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Failed result must carry an error.");
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    [JsonConstructor]
    protected internal Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of failed result.");

    public static implicit operator Result<TValue>(TValue value) => Success(value);
}

public record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided", ErrorType.Validation);
}

public enum ErrorType { None, Validation, NotFound, Conflict, Unauthorized, Forbidden, Failure }