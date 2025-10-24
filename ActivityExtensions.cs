using System.Diagnostics;
using OpenTelemetry.Trace;

namespace JaegerDemo.Api;

/// <summary>
/// Extension methods for Activity to reduce boilerplate
/// </summary>
public static class ActivityExtensions
{
    /// <summary>
    /// Record exception and set error status in one call
    /// </summary>
    public static Activity? RecordError(this Activity? activity, Exception exception, string? message = null)
    {
        if (activity == null) return null;
        
        activity.SetStatus(ActivityStatusCode.Error, message ?? exception.Message);
        activity.AddException(exception); // Use AddException instead of RecordException
        activity.AddEvent(new ActivityEvent($"Error: {exception.Message}"));
        
        return activity;
    }
    
    /// <summary>
    /// Add event and log at the same time
    /// </summary>
    public static Activity? LogEvent(this Activity? activity, string eventName, ILogger? logger = null, LogLevel logLevel = LogLevel.Information)
    {
        if (activity == null) return null;
        
        activity.AddEvent(new ActivityEvent(eventName));
        logger?.Log(logLevel, "{Event}", eventName);
        
        return activity;
    }
    
    /// <summary>
    /// Set success status
    /// </summary>
    public static Activity? SetSuccess(this Activity? activity)
    {
        activity?.SetStatus(ActivityStatusCode.Ok);
        return activity;
    }
}
