namespace Acme.SaaS.Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Message { get; }
    public string[]? Errors { get; }

    private Result(bool isSuccess, T? data, string? message, string[]? errors)
    {
        IsSuccess = isSuccess;
        Data = data;
        Message = message;
        Errors = errors;
    }

    public static Result<T> Success(T data, string? message = null) =>
        new(true, data, message, null);

    public static Result<T> Failure(string message, string[]? errors = null) =>
        new(false, default, message, errors);

    public static Result<T> Failure(string message, string error) =>
        new(false, default, message, new[] { error });
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Message { get; }
    public string[]? Errors { get; }

    private Result(bool isSuccess, string? message, string[]? errors)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors;
    }

    public static Result Success(string? message = null) =>
        new(true, message, null);

    public static Result Failure(string message, string[]? errors = null) =>
        new(false, message, errors);
}
