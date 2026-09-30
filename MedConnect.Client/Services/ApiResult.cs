namespace MedConnect.Client.Services;

public record ApiResult<T>(bool Succeeded, string? Error, T? Value)
{
    public static ApiResult<T> Ok(T value) => new(true, null, value);
    public static ApiResult<T> Fail(string error) => new(false, error, default);
}

public record ApiErrorResponse(string? Error);
