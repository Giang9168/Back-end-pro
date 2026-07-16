namespace Application.Common.Models;

public class Result<T>
{
    private Result(bool isSuccess, T? data, string? errorMessage, string? errorCode, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        if (errors != null) Errors = errors;
    }

    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorCode { get; private set; }
    public List<string> Errors { get; private set; } = new();

    public static Result<T> Success(T data)
    {
        return new Result<T>(true, data, null, null);
    }

    public static Result<T> Failure(string errorMessage, string? errorCode = null)
    {
        return new Result<T>(false, default, errorMessage, errorCode);
    }

    public static Result<T> Failure(List<string> errors, string? errorCode = null)
    {
        return new Result<T>(false, default, null, errorCode, errors);
    }
}

public class Result
{
    private Result(bool isSuccess, string? errorMessage, string? errorCode, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        if (errors != null) Errors = errors;
    }

    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorCode { get; private set; }
    public List<string> Errors { get; private set; } = new();

    public static Result Success()
    {
        return new Result(true, null, null);
    }

    public static Result Failure(string errorMessage, string? errorCode = null)
    {
        return new Result(false, errorMessage, errorCode);
    }

    public static Result Failure(List<string> errors, string? errorCode = null)
    {
        return new Result(false, null, errorCode, errors);
    }
}