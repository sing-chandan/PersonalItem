namespace InventoryProc.SharedKernel.Common;

/// <summary>
/// Result pattern for operation outcomes
/// </summary>
public class Result
{
    public bool Success { get; protected set; }
    public string Message { get; protected set; }
    public List<string> Errors { get; protected set; }

    protected Result(bool success, string message, List<string>? errors = null)
    {
        Success = success;
        Message = message;
        Errors = errors ?? new List<string>();
    }

    public static Result Ok(string message = "Operation successful")
        => new Result(true, message);

    public static Result Fail(string message, List<string>? errors = null)
        => new Result(false, message, errors);

    public static Result Fail(string message, string error)
        => new Result(false, message, new List<string> { error });
}

/// <summary>
/// Generic result pattern with data
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; private set; }

    private Result(bool success, string message, T? data = default, List<string>? errors = null)
        : base(success, message, errors)
    {
        Data = data;
    }

    public static Result<T> Ok(T data, string message = "Operation successful")
        => new Result<T>(true, message, data);

    public static new Result<T> Fail(string message, List<string>? errors = null)
        => new Result<T>(false, message, default, errors);

    public static new Result<T> Fail(string message, string error)
        => new Result<T>(false, message, default, new List<string> { error });
}
