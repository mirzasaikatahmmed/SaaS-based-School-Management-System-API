using System.Net;
using System.Text.Json;
using FluentValidation;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.Common.Wrappers;

namespace SchoolManagement.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            AppException appEx => appEx.StatusCode,
            ValidationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var message = exception switch
        {
            AppException appEx => appEx.Message,
            ValidationException => "Validation failed.",
            UnauthorizedAccessException => "Unauthorized.",
            _ => "An unexpected error occurred."
        };

        List<string>? errors = exception switch
        {
            AppException appEx => appEx.Errors,
            ValidationException vex => vex.Errors.Select(e => e.ErrorMessage).ToList(),
            _ => null
        };

        if (statusCode >= 500)
            _logger.LogError(exception, "Unhandled exception");
        else
            _logger.LogWarning(exception, "Handled exception: {Message}", message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ApiResponse.Fail(message, errors);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
