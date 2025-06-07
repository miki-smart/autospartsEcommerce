using Identity.Domain.Entities;

namespace Identity.Domain.Repositories;

public interface IDeviceRepository
{
    Task<Device?> GetByDeviceIdentifierAsync(string userId, string deviceIdentifier);
    Task AddAsync(Device device);
    Task UpdateAsync(Device device);
    Task SaveChangesAsync();
}
