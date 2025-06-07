using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using Identity.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories;

public class TwoFactorTokenRepository : ITwoFactorTokenRepository
{
    private readonly ApplicationDbContext _context;

    public TwoFactorTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TwoFactorToken>> GetUnusedTokensAsync(string userId, string type)
    {
        return await _context.TwoFactorTokens
            .Where(t => t.UserId == userId && t.Type == type && !t.IsUsed)
            .ToListAsync();
    }

    public async Task AddAsync(TwoFactorToken token)
    {
        await _context.TwoFactorTokens.AddAsync(token);
    }

    public async Task<TwoFactorToken?> GetValidTokenAsync(string userId, string tokenValue, string type)
    {
        return await _context.TwoFactorTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && 
                                      t.Token == tokenValue && 
                                      t.Type == type && 
                                      !t.IsUsed && 
                                      t.ExpiresAt > DateTime.UtcNow);
    }

    public async Task UpdateAsync(TwoFactorToken token)
    {
        _context.TwoFactorTokens.Update(token);
    }

    public async Task<IEnumerable<TwoFactorToken>> GetExpiredTokensAsync()
    {
        return await _context.TwoFactorTokens
            .Where(t => t.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task RemoveRangeAsync(IEnumerable<TwoFactorToken> tokens)
    {
        _context.TwoFactorTokens.RemoveRange(tokens);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
