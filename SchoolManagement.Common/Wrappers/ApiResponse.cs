namespace SchoolManagement.Common.Wrappers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string message = "Success") => new()
    {
        Success = true,
        Message = message,
        Data = data,
        Errors = null,
        Timestamp = DateTime.UtcNow
    };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Data = default,
        Errors = errors,
        Timestamp = DateTime.UtcNow
    };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message = "Success") => new()
    {
        Success = true,
        Message = message,
        Data = null,
        Errors = null,
        Timestamp = DateTime.UtcNow
    };

    public new static ApiResponse Fail(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Data = null,
        Errors = errors,
        Timestamp = DateTime.UtcNow
    };
}
