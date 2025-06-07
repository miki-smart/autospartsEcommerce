using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Identity.Application.Common.Configuration;
using Identity.Application.Common.Models;
using Identity.Domain.Entities;
using Identity.Domain.Constants;
using Identity.Domain.Configuration;

namespace Identity.Application.Services.Implementation;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<ApplicationUser> _userManager;

    public TokenService(
        IOptions<JwtSettings> jwtSettings,
        UserManager<ApplicationUser> userManager)
    {
        _jwtSettings = jwtSettings.Value;
        _userManager = userManager;
    }

    public async Task<TokenResult> GenerateTokenAsync(ApplicationUser user, DeviceInfo deviceInfo)
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        var userClaims = await _userManager.GetClaimsAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.UserName!),
            new("firstName", user.FirstName ?? string.Empty),
            new("lastName", user.LastName ?? string.Empty),
            new("fullName", user.FullName),
            new("emailConfirmed", user.EmailConfirmed.ToString()),
            new("twoFactorEnabled", user.TwoFactorEnabled.ToString()),
            new("isActive", user.IsActive.ToString()),
            new("deviceId", deviceInfo.DeviceId),
            new("ipAddress", deviceInfo.IpAddress),
            new("platform", deviceInfo.Platform ?? string.Empty),
            new("isTrusted", deviceInfo.IsTrusted.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        // Add role claims
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add user claims
        claims.AddRange(userClaims);

        // Add permission claims based on roles
        await AddPermissionClaimsAsync(claims, userRoles);

        var accessToken = GenerateAccessToken(claims);
        var refreshToken = GenerateRefreshToken();

        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        return new TokenResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiration = accessTokenExpiration,
            RefreshTokenExpiration = refreshTokenExpiration,
            TokenType = "Bearer"
        };
    }
    public async Task<TokenResult?> RefreshTokenAsync(string refreshToken, DeviceInfo deviceInfo)
    {
        // This method would typically validate the refresh token against the database
        // For now, we'll implement basic validation

        if (string.IsNullOrEmpty(refreshToken))
            return null;

        // TODO: Validate refresh token against database
        // TODO: Check if refresh token is expired or revoked
        // TODO: Get user from refresh token

        // For implementation, we'll need to store refresh tokens in database
        // and validate them properly

        return await Task.FromResult<TokenResult?>(null); // Placeholder - will be completed when persistence layer is ready
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = _jwtSettings.ValidateIssuer,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = _jwtSettings.ValidateAudience,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = _jwtSettings.ValidateLifetime,
                ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkewMinutes)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        // TODO: Mark refresh token as revoked in database
        return await Task.FromResult(true);
    }

    public async Task<bool> RevokeAllUserTokensAsync(string userId)
    {
        // TODO: Mark all user's refresh tokens as revoked in database
        return await Task.FromResult(true);
    }

    private string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase32String(randomNumber);
    }
    private async Task AddPermissionClaimsAsync(List<Claim> claims, IList<string> userRoles)
    {
        var allPermissions = new HashSet<string>();

        foreach (var role in userRoles)
        {
            if (DefaultRolePermissions.RolePermissionMappings.TryGetValue(role, out var permissions))
            {
                foreach (var permission in permissions)
                {
                    allPermissions.Add(permission);
                }
            }
        }

        foreach (var permission in allPermissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        await Task.CompletedTask;
    }
}
