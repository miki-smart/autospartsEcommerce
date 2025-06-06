using Identity.Application.Common.Models;
using Identity.Application.Features.Auth.Commands;
using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Auth.Handlers;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponse<bool>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<LogoutCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userManager = userManager;
        _logger = logger;
    }    public async Task<ApiResponse<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find and revoke the specific refresh token if provided
            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, request.UserId);

                if (refreshToken != null)
                {
                    refreshToken.IsRevoked = true;
                    refreshToken.RevokedAt = DateTime.UtcNow;
                    await _refreshTokenRepository.UpdateAsync(refreshToken);
                    _logger.LogInformation("Refresh token revoked for user {UserId}", request.UserId);
                }
            }
            else
            {
                // Revoke all refresh tokens for the user if no specific token provided
                var userRefreshTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(request.UserId);

                foreach (var token in userRefreshTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                    await _refreshTokenRepository.UpdateAsync(token);
                }

                _logger.LogInformation("All refresh tokens revoked for user {UserId}", request.UserId);
            }

            // Update user's security stamp to invalidate any existing tokens
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user != null)
            {
                await _userManager.UpdateSecurityStampAsync(user);
                _logger.LogInformation("Security stamp updated for user {UserId}", request.UserId);
            }

            await _refreshTokenRepository.SaveChangesAsync();

            _logger.LogInformation("User {UserId} logged out successfully", request.UserId);

            return new ApiResponse<bool>(true) { Message = "Logged out successfully" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", request.UserId);
            return ApiResponse<bool>.CreateError("Logout failed");
        }
    }
}
