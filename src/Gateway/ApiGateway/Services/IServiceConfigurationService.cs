using ApiGateway.Models;

namespace ApiGateway.Services;

public interface IServiceConfigurationService
{
    Task<ServiceManagementResponse> GetAllServicesAsync();
    Task<ServiceManagementResponse> GetServiceByIdAsync(string serviceId);
    Task<ServiceManagementResponse> AddServiceAsync(ServiceConfiguration service);
    Task<ServiceManagementResponse> UpdateServiceAsync(string serviceId, ServiceConfiguration service);
    Task<ServiceManagementResponse> DeleteServiceAsync(string serviceId);
    Task<ServiceManagementResponse> ToggleServiceAsync(string serviceId);
    Task<ServiceHealthStatus> CheckServiceHealthAsync(ServiceConfiguration service);
    Task<bool> ReloadOcelotConfigurationAsync();
}
