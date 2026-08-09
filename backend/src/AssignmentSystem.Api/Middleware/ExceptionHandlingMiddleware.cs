using System.Net;
using System.Text.Json;
using AssignmentSystem.Core.Interfaces;

namespace AssignmentSystem.Api.Middleware;

// Centralized error handling so controllers stay free of try/catch noise, and every
// error returns a consistent JSON shape: { "error": "message" }.
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
            var (statusCode, message) = ex switch
            {
                NotFoundException => (HttpStatusCode.NotFound, ex.Message),
                ForbiddenActionException => (HttpStatusCode.Forbidden, ex.Message),
                BusinessRuleException => (HttpStatusCode.BadRequest, ex.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ex.Message),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            if (statusCode == HttpStatusCode.InternalServerError)
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            else
                _logger.LogWarning("{ExceptionType}: {Message}", ex.GetType().Name, ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var payload = JsonSerializer.Serialize(new { error = message });
            await context.Response.WriteAsync(payload);
        }
    }
}
