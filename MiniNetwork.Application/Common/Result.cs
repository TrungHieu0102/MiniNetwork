namespace MiniNetwork.Application.Common;

public class Result
{
    public bool Succeeded { get; init; }
    public string? Error { get; init; }

    public static Result Success() => new() { Succeeded = true };
    public static Result Failure(string error) => new() { Succeeded = false, Error = error };
}

public class Result<T>
{
    public bool Succeeded { get; init; }
    public string? Error { get; init; }
    public T? Data { get; init; }

    public static Result<T> Success(T data) => new() { Succeeded = true, Data = data };
    public static Result<T> Failure(string error) => new() { Succeeded = false, Error = error };
}
