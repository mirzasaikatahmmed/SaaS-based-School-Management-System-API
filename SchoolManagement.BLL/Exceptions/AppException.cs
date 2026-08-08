namespace SchoolManagement.BLL.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }
    public List<string>? Errors { get; }

    public AppException(string message, int statusCode = 400, List<string>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404) { }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized") : base(message, 401) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Forbidden") : base(message, 403) { }
}

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409) { }
}
