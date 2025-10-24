using System.Diagnostics;
using OpenTelemetry.Trace;

namespace JaegerDemo.Api;

/// <summary>
/// Middleware that automatically captures unhandled exceptions and records them in the current Activity
/// </summary>
public class ExceptionTracingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionTracingMiddleware> _logger;

    public ExceptionTracingMiddleware(RequestDelegate next, ILogger<ExceptionTracingMiddleware> logger)
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
            // Automatically record exception in current activity
            var activity = Activity.Current;
            if (activity != null)
            {
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity.AddException(ex);  // Use AddException instead of RecordException
                activity.AddEvent(new ActivityEvent($"Unhandled exception: {ex.GetType().Name}"));
            }

            _logger.LogError(ex, "Unhandled exception in request {Method} {Path}", 
                context.Request.Method, context.Request.Path);

            // Re-throw to let ASP.NET Core error handling take over
            throw;
        }
    }
}

/// <summary>
/// Extension method to register the middleware
/// </summary>
public static class ExceptionTracingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionTracing(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionTracingMiddleware>();
    }
}
