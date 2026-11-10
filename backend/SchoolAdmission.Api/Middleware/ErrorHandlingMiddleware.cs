using System.Text.Json;
using SchoolAdmission.Api.DTOs;

namespace SchoolAdmission.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (BadRequestException ex)
        {
            _logger.LogWarning("Bad request: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status400BadRequest, new ErrorResponse("Bad request", ex.Message));
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Not found: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status404NotFound, new ErrorResponse("Not found", ex.Message));
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning("Conflict: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status409Conflict, new ErrorResponse("Conflict", ex.Message));
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning("Forbidden: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status403Forbidden, new ErrorResponse("Forbidden", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteResponseAsync(context, StatusCodes.Status500InternalServerError, new ErrorResponse("Internal server error", "An unexpected error occurred."));
        }
    }

    private static async Task WriteResponseAsync(HttpContext context, int statusCode, object value)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
        await context.Response.WriteAsync(json);
    }
}
