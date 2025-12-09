using System.Diagnostics;
using System.Text;

namespace Api.Infrastructure.Telemetry;

/// <summary>
/// Middleware that logs HTTP request telemetry with emoticons to the console
/// </summary>
public class HttpRequestTelemetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpRequestTelemetryMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpRequestTelemetryMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public HttpRequestTelemetryMiddleware(RequestDelegate next, ILogger<HttpRequestTelemetryMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to log HTTP request telemetry.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestMethod = context.Request.Method;
        var requestPath = context.Request.Path;
        var requestId = Guid.NewGuid().ToString("N")[..8];

        // Log incoming request with appropriate emoticon
        var methodEmoticon = GetMethodEmoticon(requestMethod);
        Console.WriteLine($"{methodEmoticon} [{requestId}] {requestMethod} {requestPath} - Request started");

        // Capture and log request body for POST requests
        if (requestMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            await LogRequestBodyAsync(context, requestId);
        }

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

    private async Task LogRequestBodyAsync(HttpContext context, string requestId)
    {
        try
        {
            // Enable buffering so the request body can be read multiple times
            context.Request.EnableBuffering();

            // Read the request body
            using var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();

            // Reset the request body stream position so it can be read again by downstream middleware
            context.Request.Body.Position = 0;

            // Log the request body
            if (!string.IsNullOrWhiteSpace(body))
            {
                Console.WriteLine($"📝 [{requestId}] Request Body: {body}");
            }
            else
            {
                Console.WriteLine($"📝 [{requestId}] Request Body: (empty)");
            }
        }
        catch (Exception ex)
        {
            // Log any errors reading the body, but don't fail the request
            Console.WriteLine($"⚠️ [{requestId}] Failed to read request body: {ex.Message}");
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
