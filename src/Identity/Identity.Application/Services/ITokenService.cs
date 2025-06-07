using Identity.Domain.Entities;
using Identity.Application.Common.Models;
using System.Security.Claims;

namespace Identity.Application.Services;

/// <summary>
/// Interface for JWT token generation and validation services
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates JWT access and refresh tokens for the specified user and device
    /// </summary>
    /// <param name="user">The user to generate tokens for</param>
    /// <param name="deviceInfo">Device information</param>
    /// <returns>Token result containing access and refresh tokens</returns>
    Task<TokenResult> GenerateTokenAsync(ApplicationUser user, DeviceInfo deviceInfo);

    /// <summary>
    /// Refreshes tokens using a valid refresh token
    /// </summary>
    /// <param name="refreshToken">The refresh token</param>
    /// <param name="deviceInfo">Device information</param>
    /// <returns>New token result or null if invalid</returns>
    Task<TokenResult?> RefreshTokenAsync(string refreshToken, DeviceInfo deviceInfo);

    /// <summary>
    /// Validates a JWT token and returns the claims principal
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>Claims principal or null if invalid</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Revokes a specific refresh token
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke</param>
    /// <returns>True if successful</returns>
    Task<bool> RevokeTokenAsync(string refreshToken);

    /// <summary>
    /// Revokes all refresh tokens for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>True if successful</returns>
    Task<bool> RevokeAllUserTokensAsync(string userId);
}
