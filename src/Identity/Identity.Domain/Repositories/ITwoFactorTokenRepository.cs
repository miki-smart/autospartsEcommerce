using Identity.Domain.Entities;

namespace Identity.Domain.Repositories;

public interface ITwoFactorTokenRepository
{
    Task<IEnumerable<TwoFactorToken>> GetUnusedTokensAsync(string userId, string type);
    Task AddAsync(TwoFactorToken token);
    Task<TwoFactorToken?> GetValidTokenAsync(string userId, string tokenValue, string type);
    Task UpdateAsync(TwoFactorToken token);
    Task<IEnumerable<TwoFactorToken>> GetExpiredTokensAsync();
    Task RemoveRangeAsync(IEnumerable<TwoFactorToken> tokens);
    Task SaveChangesAsync();
}
