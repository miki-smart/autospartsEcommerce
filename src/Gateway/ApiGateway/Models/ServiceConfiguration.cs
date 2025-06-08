using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Models;

public class ServiceConfiguration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string ServiceName { get; set; } = string.Empty;
    
    [Required]
    public string UpstreamPathTemplate { get; set; } = string.Empty;
    
    [Required]
    public string DownstreamPathTemplate { get; set; } = string.Empty;
    
    [Required]
    public string DownstreamScheme { get; set; } = "http";
    
    public string? DownstreamHost { get; set; }
    
    public int? DownstreamPort { get; set; }
    
    public List<string> UpstreamHttpMethods { get; set; } = new() { "GET" };
    
    public bool RequireAuthentication { get; set; } = false;
    
    public bool EnableRateLimiting { get; set; } = false;
    
    public int RateLimitPeriodInSeconds { get; set; } = 60;
    
    public int RateLimitCount { get; set; } = 100;
    
    public bool EnableQoS { get; set; } = false;
    
    public int QoSExceptionsAllowedBeforeBreaking { get; set; } = 3;
    
    public int QoSDurationOfBreakInMs { get; set; } = 30000;
    
    public int QoSTimeoutValueInMs { get; set; } = 10000;
    
    public string LoadBalancerType { get; set; } = "LeastConnection";
    
    public bool RouteIsCaseSensitive { get; set; } = false;
    
    public bool DangerousAcceptAnyServerCertificateValidator { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public string CreatedBy { get; set; } = "Admin";
    
    public bool IsActive { get; set; } = true;
    
    public string? Description { get; set; }
    
    public Dictionary<string, string> Tags { get; set; } = new();
}

public class ServiceHealthStatus
{
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime LastChecked { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ResponseTime { get; set; }
}

public class ServiceManagementRequest
{
    [Required]
    public string Action { get; set; } = string.Empty; // "add", "update", "delete", "toggle"
    
    public ServiceConfiguration? Service { get; set; }
    
    public string? ServiceId { get; set; }
}

public class ServiceManagementResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ServiceConfiguration? Service { get; set; }
    public List<ServiceConfiguration>? Services { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
