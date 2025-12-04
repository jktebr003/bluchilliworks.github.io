using System.Diagnostics;

namespace Api.Infrastructure.Telemetry;

/// <summary>
/// Middleware that logs HTTP request telemetry with emoticons to the console
/// </summary>
public class HttpRequestTelemetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpRequestTelemetryMiddleware> _logger;

    public HttpRequestTelemetryMiddleware(RequestDelegate next, ILogger<HttpRequestTelemetryMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestMethod = context.Request.Method;
        var requestPath = context.Request.Path;
        var requestId = Guid.NewGuid().ToString("N")[..8];

        // Log incoming request with appropriate emoticon
        var methodEmoticon = GetMethodEmoticon(requestMethod);
        Console.WriteLine($"{methodEmoticon} [{requestId}] {requestMethod} {requestPath} - Request started");

        try
        {
            // Call the next middleware in the pipeline
            await _next(context);

            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Log response with status-based emoticon
            var statusEmoticon = GetStatusEmoticon(statusCode);
            Console.WriteLine($"{statusEmoticon} [{requestId}] {requestMethod} {requestPath} - {statusCode} completed in {elapsedMs}ms");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Log error with error emoticon
            Console.WriteLine($"💥 [{requestId}] {requestMethod} {requestPath} - ERROR after {elapsedMs}ms");
            Console.WriteLine($"❌ [{requestId}] Exception: {ex.GetType().Name} - {ex.Message}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"🔍 [{requestId}] Inner Exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
            }

            // Re-throw to let the error handling middleware process it
            throw;
        }
    }

    private static string GetMethodEmoticon(string method)
    {
        return method.ToUpperInvariant() switch
        {
            "GET" => "📥",      // Incoming/receiving data
            "POST" => "📤",     // Outgoing/sending data
            "PUT" => "✏️",      // Editing/updating
            "PATCH" => "🔧",    // Modifying/fixing
            "DELETE" => "🗑️",   // Removing
            "HEAD" => "🔍",     // Inspecting
            "OPTIONS" => "❓",   // Querying options
            _ => "📡"           // Generic request
        };
    }

    private static string GetStatusEmoticon(int statusCode)
    {
        return statusCode switch
        {
            >= 200 and < 300 => "✅",  // Success
            >= 300 and < 400 => "↪️",   // Redirect
            401 or 403 => "🔒",        // Authentication/Authorization
            404 => "🔍",               // Not found
            >= 400 and < 500 => "⚠️",   // Client error
            >= 500 => "💥",            // Server error
            _ => "❔"                  // Unknown status
        };
    }
}
