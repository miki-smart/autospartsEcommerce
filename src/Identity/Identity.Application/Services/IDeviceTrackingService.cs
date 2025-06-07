using Identity.Application.Common.Models;
using Identity.Domain.Entities;

namespace Identity.Application.Services;

public interface IDeviceTrackingService
{
    Task<bool> IsDeviceTrustedAsync(string userId, string deviceId);
    Task<DeviceInfo> CreateDeviceInfoAsync(string deviceId, string ipAddress, string userAgent, string? platform = null, string? browser = null, string? os = null);
    Task<bool> AddTrustedDeviceAsync(string userId, DeviceInfo deviceInfo);
    Task<bool> RemoveTrustedDeviceAsync(string userId, string deviceId);
    Task<List<Device>> GetUserDevicesAsync(string userId);
    Task<bool> IsNewDeviceAsync(string userId, string deviceId);
    Task LogDeviceAccessAsync(string userId, DeviceInfo deviceInfo, bool isSuccessful);
}
