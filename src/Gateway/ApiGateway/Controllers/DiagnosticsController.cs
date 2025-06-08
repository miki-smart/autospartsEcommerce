using Microsoft.AspNetCore.Mvc;
using ApiGateway.Middleware;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(ILogger<DiagnosticsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Simple endpoint to test correlation ID functionality
    /// </summary>
    [HttpGet("test-correlation")]
    public IActionResult TestCorrelation()
    {
        var correlationId = HttpContext.GetCorrelationId();
        
        _logger.LogInformation("Correlation ID test endpoint called");
        
        return Ok(new
        {
            CorrelationId = correlationId,
            Message = "Correlation ID is working properly",
            Timestamp = DateTime.UtcNow,
            RequestHeaders = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
        });
    }

    /// <summary>
    /// Endpoint to test that correlation ID is propagated in response headers
    /// </summary>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        _logger.LogInformation("API Gateway ping endpoint called");
        
        return Ok(new
        {
            Message = "API Gateway is running",
            CorrelationId = HttpContext.GetCorrelationId(),
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        });
    }
}
