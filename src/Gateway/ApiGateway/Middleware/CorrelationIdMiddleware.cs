using Serilog.Context;

namespace ApiGateway.Middleware;

/// <summary>
/// Custom middleware that generates and propagates correlation IDs across requests.
/// This replaces the CorrelationId package functionality with a simple, reliable implementation.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        // Add correlation ID to the response headers so clients can track requests
        context.Response.Headers[CorrelationIdHeader] = correlationId;
        
        // Add correlation ID to HttpContext.Items for use by other middleware/controllers
        context.Items["CorrelationId"] = correlationId;
        
        // Add correlation ID to Serilog logging context
        // This ensures all log entries for this request include the correlation ID
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation("Processing request {Method} {Path} with correlation ID {CorrelationId}", 
                context.Request.Method, context.Request.Path, correlationId);
            
            await _next(context);
            
            _logger.LogInformation("Completed request {Method} {Path} with correlation ID {CorrelationId} - Status: {StatusCode}", 
                context.Request.Method, context.Request.Path, correlationId, context.Response.StatusCode);
        }
    }

    /// <summary>
    /// Gets existing correlation ID from request headers or creates a new one.
    /// This ensures that correlation IDs propagate through the entire microservices chain.
    /// </summary>
    private string GetOrCreateCorrelationId(HttpContext context)
    {
        // First, check if correlation ID exists in request headers (from upstream services or clients)
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var existingId) && 
            !string.IsNullOrWhiteSpace(existingId))
        {
            return existingId.ToString();
        }

        // If no correlation ID exists, generate a new one
        // Using a shorter format for better readability in logs
        return Guid.NewGuid().ToString("N")[..12]; // Results in format like: abc123def456
    }
}

/// <summary>
/// Extension methods to make it easy to register and use the correlation ID middleware
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    /// <summary>
    /// Adds the correlation ID middleware to the pipeline.
    /// Should be added early in the pipeline to ensure all subsequent middleware can use the correlation ID.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }

    /// <summary>
    /// Gets the correlation ID for the current request from HttpContext.Items
    /// </summary>
    public static string? GetCorrelationId(this HttpContext context)
    {
        return context.Items["CorrelationId"]?.ToString();
    }
}
