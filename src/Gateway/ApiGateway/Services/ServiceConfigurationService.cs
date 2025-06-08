using System.Text.Json;
using ApiGateway.Models;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Services;

public class ServiceConfigurationService : IServiceConfigurationService
{
    private readonly string _ocelotConfigPath;
    private readonly ILogger<ServiceConfigurationService> _logger;
    private List<ServiceConfiguration> _services = new();

    public ServiceConfigurationService(ILogger<ServiceConfigurationService> logger)
    {
        _logger = logger;
        _ocelotConfigPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "ocelot.json");
        LoadServicesFromOcelot();
    }

    private void LoadServicesFromOcelot()
    {
        if (!File.Exists(_ocelotConfigPath))
        {
            _logger.LogWarning("Ocelot config not found at {Path}", _ocelotConfigPath);
            _services = new List<ServiceConfiguration>();
            return;
        }
        var json = File.ReadAllText(_ocelotConfigPath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.TryGetProperty("Routes", out var routes))
        {
            _services = routes.EnumerateArray().Select(route =>
            {
                var svc = new ServiceConfiguration
                {
                    ServiceName = route.GetPropertyOrDefault("ServiceName", ""),
                    UpstreamPathTemplate = route.GetPropertyOrDefault("UpstreamPathTemplate", ""),
                    DownstreamPathTemplate = route.GetPropertyOrDefault("DownstreamPathTemplate", ""),
                    DownstreamScheme = route.GetPropertyOrDefault("DownstreamScheme", "http"),
                    DownstreamHost = route.TryGetProperty("DownstreamHostAndPort", out var hostPort) ? hostPort.GetPropertyOrDefault("Host", null) : null,
                    DownstreamPort = route.TryGetProperty("DownstreamHostAndPort", out var hostPort2) ? hostPort2.GetPropertyOrDefault("Port", 0) : 0,
                    UpstreamHttpMethods = route.TryGetProperty("UpstreamHttpMethod", out var methods) ? methods.EnumerateArray().Select(m => m.GetString() ?? "GET").ToList() : new List<string> { "GET" },
                    RequireAuthentication = route.TryGetProperty("AuthenticationOptions", out _) ? true : false,
                    EnableRateLimiting = route.TryGetProperty("RateLimitOptions", out var rate) && rate.GetPropertyOrDefault("EnableRateLimiting", false),
                    RateLimitPeriodInSeconds = route.TryGetProperty("RateLimitOptions", out var rate2) ? ParsePeriod(rate2.GetPropertyOrDefault("Period", "1s")) : 60,
                    RateLimitCount = route.TryGetProperty("RateLimitOptions", out var rate3) ? rate3.GetPropertyOrDefault("Limit", 100) : 100,
                    EnableQoS = route.TryGetProperty("QoSOptions", out var qos) && qos.ValueKind != JsonValueKind.Null,
                    QoSExceptionsAllowedBeforeBreaking = route.TryGetProperty("QoSOptions", out var qos2) ? qos2.GetPropertyOrDefault("ExceptionsAllowedBeforeBreaking", 3) : 3,
                    QoSDurationOfBreakInMs = route.TryGetProperty("QoSOptions", out var qos3) ? qos3.GetPropertyOrDefault("DurationOfBreak", 30000) : 30000,
                    QoSTimeoutValueInMs = route.TryGetProperty("QoSOptions", out var qos4) ? qos4.GetPropertyOrDefault("TimeoutValue", 10000) : 10000,
                    LoadBalancerType = route.TryGetProperty("LoadBalancerOptions", out var lb) ? lb.GetPropertyOrDefault("Type", "LeastConnection") : "LeastConnection",
                    RouteIsCaseSensitive = route.GetPropertyOrDefault("RouteIsCaseSensitive", false),
                    DangerousAcceptAnyServerCertificateValidator = route.GetPropertyOrDefault("DangerousAcceptAnyServerCertificateValidator", true),
                    IsActive = true
                };
                return svc;
            }).ToList();
        }
    }

    private int ParsePeriod(string period)
    {
        if (period.EndsWith("s") && int.TryParse(period.TrimEnd('s'), out var s)) return s;
        if (period.EndsWith("m") && int.TryParse(period.TrimEnd('m'), out var m)) return m * 60;
        if (period.EndsWith("h") && int.TryParse(period.TrimEnd('h'), out var h)) return h * 3600;
        return 60;
    }

    public Task<ServiceManagementResponse> GetAllServicesAsync()
    {
        return Task.FromResult(new ServiceManagementResponse
        {
            Success = true,
            Services = _services.ToList(),
            Message = $"Loaded {_services.Count} services"
        });
    }

    public Task<ServiceManagementResponse> GetServiceByIdAsync(string serviceId)
    {
        var svc = _services.FirstOrDefault(s => s.Id == serviceId);
        return Task.FromResult(new ServiceManagementResponse
        {
            Success = svc != null,
            Service = svc,
            Message = svc != null ? "Service found" : "Service not found"
        });
    }

    public Task<ServiceManagementResponse> AddServiceAsync(ServiceConfiguration service)
    {
        service.Id = Guid.NewGuid().ToString();
        _services.Add(service);
        SaveServicesToOcelot();
        return Task.FromResult(new ServiceManagementResponse
        {
            Success = true,
            Service = service,
            Services = _services.ToList(),
            Message = "Service added"
        });
    }

    public Task<ServiceManagementResponse> UpdateServiceAsync(string serviceId, ServiceConfiguration service)
    {
        var idx = _services.FindIndex(s => s.Id == serviceId);
        if (idx == -1)
        {
            return Task.FromResult(new ServiceManagementResponse { Success = false, Message = "Service not found" });
        }
        service.Id = serviceId;
        _services[idx] = service;
        SaveServicesToOcelot();
        return Task.FromResult(new ServiceManagementResponse
        {
            Success = true,
            Service = service,
            Services = _services.ToList(),
            Message = "Service updated"
        });
    }

    public Task<ServiceManagementResponse> DeleteServiceAsync(string serviceId)
    {
        var svc = _services.FirstOrDefault(s => s.Id == serviceId);
        if (svc == null)
        {
            return Task.FromResult(new ServiceManagementResponse { Success = false, Message = "Service not found" });
        }
        _services.Remove(svc);
        SaveServicesToOcelot();
        return Task.FromResult(new ServiceManagementResponse
        {
            Success = true,
            Service = svc,
            Services = _services.ToList(),
            Message = "Service deleted"
        });
    }

    public Task<ServiceManagementResponse> ToggleServiceAsync(string serviceId)
    {
        var svc = _services.FirstOrDefault(s => s.Id == serviceId);
        if (svc == null)
        {
            return Task.FromResult(new ServiceManagementResponse { Success = false, Message = "Service not found" });
        }
        svc.IsActive = !svc.IsActive;
        SaveServicesToOcelot();
        return Task.FromResult(new ServiceManagementResponse
        {
            Success = true,
            Service = svc,
            Services = _services.ToList(),
            Message = $"Service {(svc.IsActive ? "enabled" : "disabled")}" 
        });
    }

    public Task<ServiceHealthStatus> CheckServiceHealthAsync(ServiceConfiguration service)
    {
        // Simple health check logic (ping downstream host)
        var status = new ServiceHealthStatus
        {
            ServiceName = service.ServiceName,
            Status = "Unknown",
            Url = $"{service.DownstreamScheme}://{service.DownstreamHost}:{service.DownstreamPort}",
            LastChecked = DateTime.UtcNow
        };
        // TODO: Implement actual HTTP ping
        return Task.FromResult(status);
    }

    public Task<bool> ReloadOcelotConfigurationAsync()
    {
        // Ocelot will reload config on file change
        return Task.FromResult(true);
    }

    private void SaveServicesToOcelot()
    {
        // Read the current ocelot.json
        var json = File.Exists(_ocelotConfigPath) ? File.ReadAllText(_ocelotConfigPath) : "{\"Routes\":[],\"GlobalConfiguration\":{}}";
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement.Clone();
        var globalConfig = root.TryGetProperty("GlobalConfiguration", out var gc) ? gc : default;
        var routes = new List<Dictionary<string, object?>>();
        foreach (var svc in _services)
        {
            var route = new Dictionary<string, object?>
            {
                ["ServiceName"] = svc.ServiceName,
                ["UpstreamPathTemplate"] = svc.UpstreamPathTemplate,
                ["DownstreamPathTemplate"] = svc.DownstreamPathTemplate,
                ["DownstreamScheme"] = svc.DownstreamScheme,
                ["UpstreamHttpMethod"] = svc.UpstreamHttpMethods,
                ["RouteIsCaseSensitive"] = svc.RouteIsCaseSensitive,
                ["DangerousAcceptAnyServerCertificateValidator"] = svc.DangerousAcceptAnyServerCertificateValidator
            };
            if (!string.IsNullOrEmpty(svc.DownstreamHost) && svc.DownstreamPort.HasValue)
            {
                route["DownstreamHostAndPort"] = new Dictionary<string, object?>
                {
                    ["Host"] = svc.DownstreamHost,
                    ["Port"] = svc.DownstreamPort.Value
                };
            }
            if (svc.RequireAuthentication)
            {
                route["AuthenticationOptions"] = new Dictionary<string, object?>
                {
                    ["AuthenticationProviderKey"] = "Bearer",
                    ["AllowedScopes"] = new List<string>()
                };
            }
            if (svc.EnableRateLimiting)
            {
                route["RateLimitOptions"] = new Dictionary<string, object?>
                {
                    ["EnableRateLimiting"] = true,
                    ["Period"] = $"{svc.RateLimitPeriodInSeconds}s",
                    ["Limit"] = svc.RateLimitCount
                };
            }
            if (svc.EnableQoS)
            {
                route["QoSOptions"] = new Dictionary<string, object?>
                {
                    ["ExceptionsAllowedBeforeBreaking"] = svc.QoSExceptionsAllowedBeforeBreaking,
                    ["DurationOfBreak"] = svc.QoSDurationOfBreakInMs,
                    ["TimeoutValue"] = svc.QoSTimeoutValueInMs
                };
            }
            if (!string.IsNullOrEmpty(svc.LoadBalancerType))
            {
                route["LoadBalancerOptions"] = new Dictionary<string, object?>
                {
                    ["Type"] = svc.LoadBalancerType
                };
            }
            routes.Add(route);
        }
        var newConfig = new Dictionary<string, object?>
        {
            ["Routes"] = routes,
            ["GlobalConfiguration"] = globalConfig.ValueKind != JsonValueKind.Undefined ? JsonSerializer.Deserialize<object>(globalConfig.GetRawText()) : new { }
        };
        var newJson = JsonSerializer.Serialize(newConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_ocelotConfigPath, newJson);
    }
}

// Helper extension for safe property access
public static class JsonElementExtensions
{
    public static string GetPropertyOrDefault(this JsonElement element, string property, string defaultValue)
    {
        if (element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String)
            return value.GetString() ?? defaultValue;
        return defaultValue;
    }
    public static int GetPropertyOrDefault(this JsonElement element, string property, int defaultValue)
    {
        if (element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var i))
            return i;
        return defaultValue;
    }
    public static bool GetPropertyOrDefault(this JsonElement element, string property, bool defaultValue)
    {
        if (element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.True) return true;
        if (element.TryGetProperty(property, out var value2) && value2.ValueKind == JsonValueKind.False) return false;
        return defaultValue;
    }
}
