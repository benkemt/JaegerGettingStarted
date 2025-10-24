using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using OpenTelemetry.Trace;

namespace JaegerDemo.Api;

/// <summary>
/// Exception filter that automatically records exceptions in Activities
/// Alternative to middleware - works at the endpoint level
/// </summary>
public class ActivityExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ActivityExceptionFilter> _logger;

    public ActivityExceptionFilter(ILogger<ActivityExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var activity = Activity.Current;
        
        if (activity != null)
        {
            activity.SetStatus(ActivityStatusCode.Error, context.Exception.Message);
            activity.AddException(context.Exception);  // Use AddException instead of RecordException
            activity.AddEvent(new ActivityEvent($"Exception in {context.ActionDescriptor.DisplayName}: {context.Exception.Message}"));
        }

        _logger.LogError(context.Exception, 
            "Exception in endpoint {Endpoint}", 
            context.ActionDescriptor.DisplayName);
        
        // Don't set context.ExceptionHandled = true to let normal error handling proceed
    }
}

/// <summary>
/// Extension to register the filter globally
/// </summary>
public static class ActivityExceptionFilterExtensions
{
    public static IServiceCollection AddActivityExceptionFilter(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ActivityExceptionFilter>();
        });
        
        return services;
    }
}
