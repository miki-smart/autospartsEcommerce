using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using Identity.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly ApplicationDbContext _context;

    public DeviceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Device?> GetByDeviceIdentifierAsync(string userId, string deviceIdentifier)
    {
        return await _context.Devices
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceIdentifier == deviceIdentifier);
    }

    public async Task AddAsync(Device device)
    {
        await _context.Devices.AddAsync(device);
    }

    public async Task UpdateAsync(Device device)
    {
        _context.Devices.Update(device);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
