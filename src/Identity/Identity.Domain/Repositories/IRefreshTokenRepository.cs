using Identity.Domain.Entities;

namespace Identity.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, string userId);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);
    Task<RefreshToken?> GetByDeviceIdAsync(string userId, int deviceId);
    Task AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task SaveChangesAsync();
}
