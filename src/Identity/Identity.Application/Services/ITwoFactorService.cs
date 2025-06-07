using Identity.Application.Common.Models;
using Identity.Domain.Entities;

namespace Identity.Application.Services;

public interface ITwoFactorService
{
    /// <summary>
    /// Generates and sends a two-factor authentication code
    /// </summary>
    /// <param name="user">The user requesting 2FA</param>
    /// <param name="type">Type of 2FA (Email, SMS)</param>
    /// <returns>Result indicating success or failure</returns>
    Task<ServiceResult> GenerateAndSendCodeAsync(ApplicationUser user, TwoFactorType type);
    
    /// <summary>
    /// Verifies a two-factor authentication code
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="code">The verification code</param>
    /// <param name="type">Type of 2FA</param>
    /// <returns>Result indicating success or failure</returns>
    Task<ServiceResult<bool>> VerifyCodeAsync(string userId, string code, TwoFactorType type);
    
    /// <summary>
    /// Generates a time-based one-time password (TOTP) secret for authenticator apps
    /// </summary>
    /// <param name="user">The user</param>
    /// <returns>TOTP secret and QR code data</returns>
    Task<ServiceResult<TotpSetupResult>> GenerateAuthenticatorSecretAsync(ApplicationUser user);
    
    /// <summary>
    /// Verifies a TOTP code from authenticator app
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="code">The TOTP code</param>
    /// <returns>Result indicating success or failure</returns>
    Task<ServiceResult<bool>> VerifyAuthenticatorCodeAsync(ApplicationUser user, string code);
    
    /// <summary>
    /// Cleanup expired tokens
    /// </summary>
    /// <returns>Number of tokens cleaned up</returns>
    Task<int> CleanupExpiredTokensAsync();
}

public enum TwoFactorType
{
    Email = 1,
    SMS = 2,
    Authenticator = 3
}

public class TotpSetupResult
{
    public string Secret { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
    public string ManualEntryKey { get; set; } = string.Empty;
}
