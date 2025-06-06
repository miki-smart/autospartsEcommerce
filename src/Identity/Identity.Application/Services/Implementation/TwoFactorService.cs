using Identity.Application.Common.Models;
using Identity.Application.Services;
using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Identity.Application.Services.Implementation;

public class TwoFactorService : ITwoFactorService
{
    private readonly ITwoFactorTokenRepository _tokenRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TwoFactorService> _logger;

    public TwoFactorService(
        ITwoFactorTokenRepository tokenRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<TwoFactorService> logger)
    {
        _tokenRepository = tokenRepository;
        _userManager = userManager;
        _logger = logger;
    }    public async Task<ServiceResult> GenerateAndSendCodeAsync(ApplicationUser user, TwoFactorType type)
    {
        try
        {
            // Generate a 6-digit code
            var code = GenerateSecureCode();
            var expiresAt = DateTime.UtcNow.AddMinutes(5); // 5-minute expiration

            // Invalidate any existing unused tokens for this user and type
            var existingTokens = await _tokenRepository.GetUnusedTokensAsync(user.Id, type.ToString());

            foreach (var token in existingTokens)
            {
                token.IsUsed = true;
                await _tokenRepository.UpdateAsync(token);
            }

            // Create new token
            var twoFactorToken = new TwoFactorToken
            {
                UserId = user.Id,
                Token = code,
                Type = type.ToString(),
                ExpiresAt = expiresAt,
                CreatedDate = DateTime.UtcNow,
                IsUsed = false,
                Recipient = type == TwoFactorType.Email ? user.Email : user.PhoneNumber
            };

            await _tokenRepository.AddAsync(twoFactorToken);
            await _tokenRepository.SaveChangesAsync();

            // Send the code based on type
            var sendResult = await SendCodeAsync(user, code, type);
            if (!sendResult.IsSuccess)
            {
                return ServiceResult.Failure(sendResult.ErrorMessage ?? "Failed to send verification code");
            }            _logger.LogInformation("2FA code generated and sent for user {UserId} via {Type}", user.Id, type);
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating and sending 2FA code for user {UserId}", user.Id);
            return ServiceResult.Failure("Failed to generate verification code");
        }
    }

    public async Task<ServiceResult<bool>> VerifyCodeAsync(string userId, string code, TwoFactorType type)
    {
        try
        {
            var token = await _tokenRepository.GetValidTokenAsync(userId, code, type.ToString());

            if (token == null)
            {
                _logger.LogWarning("Invalid or expired 2FA code provided for user {UserId}", userId);
                return ServiceResult<bool>.Failure("Invalid or expired verification code");
            }

            // Mark token as used
            token.IsUsed = true;
            token.VerifiedAt = DateTime.UtcNow;
            await _tokenRepository.UpdateAsync(token);
            await _tokenRepository.SaveChangesAsync();

            _logger.LogInformation("2FA code verified successfully for user {UserId}", userId);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying 2FA code for user {UserId}", userId);
            return ServiceResult<bool>.Failure("Failed to verify code");
        }
    }    public Task<ServiceResult<TotpSetupResult>> GenerateAuthenticatorSecretAsync(ApplicationUser user)
    {
        try
        {
            // Generate a secure 32-byte secret
            var secretBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(secretBytes);
            }

            var secret = Convert.ToBase32String(secretBytes).TrimEnd('=');
            var issuer = "AutoParts Ecommerce";
            var accountName = user.Email ?? user.UserName ?? "User";

            // Generate QR code URI for authenticator apps
            var qrCodeUri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountName)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";

            // TODO: Store the secret in user's authenticator key when UserManager extensions are available
            // await _userManager.SetAuthenticatorKeyAsync(user, secret);

            var result = new TotpSetupResult
            {
                Secret = secret,
                QrCodeUri = qrCodeUri,
                ManualEntryKey = FormatSecretForManualEntry(secret)
            };

            _logger.LogInformation("TOTP secret generated for user {UserId}", user.Id);
            return Task.FromResult(ServiceResult<TotpSetupResult>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating TOTP secret for user {UserId}", user.Id);
            return Task.FromResult(ServiceResult<TotpSetupResult>.Failure("Failed to generate authenticator secret"));
        }
    }

    public async Task<ServiceResult<bool>> VerifyAuthenticatorCodeAsync(ApplicationUser user, string code)
    {
        try
        {
            // TODO: Implement when UserManager extensions are available
            // var verificationResult = await _userManager.VerifyTwoFactorTokenAsync(
            //     user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);

            // For now, return a placeholder response
            await Task.CompletedTask;
            _logger.LogWarning("TOTP verification not yet implemented for user {UserId}", user.Id);
            return ServiceResult<bool>.Failure("TOTP verification not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying TOTP code for user {UserId}", user.Id);
            return ServiceResult<bool>.Failure("Failed to verify authenticator code");
        }
    }public async Task<int> CleanupExpiredTokensAsync()
    {
        try
        {
            var expiredTokens = await _tokenRepository.GetExpiredTokensAsync();

            if (expiredTokens.Any())
            {
                await _tokenRepository.RemoveRangeAsync(expiredTokens);
                await _tokenRepository.SaveChangesAsync();
            }

            _logger.LogInformation("Cleaned up {Count} expired 2FA tokens", expiredTokens.Count());
            return expiredTokens.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired 2FA tokens");
            return 0;
        }
    }    private static string GenerateSecureCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var code = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000;
        return code.ToString("D6");
    }

    private async Task<ServiceResult> SendCodeAsync(ApplicationUser user, string code, TwoFactorType type)
    {
        // This is a placeholder implementation
        // In a real application, you would integrate with email/SMS services
        try
        {
            switch (type)
            {                case TwoFactorType.Email:
                    // TODO: Integrate with email service (e.g., SendGrid, SMTP)
                    _logger.LogInformation("Email 2FA code: {Code} (would be sent to {Email})", code, user.Email);
                    return ServiceResult.Success();

                case TwoFactorType.SMS:
                    // TODO: Integrate with SMS service (e.g., Twilio)
                    _logger.LogInformation("SMS 2FA code: {Code} (would be sent to {Phone})", code, user.PhoneNumber);
                    return ServiceResult.Success();

                default:
                    return ServiceResult.Failure("Unsupported 2FA type");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending 2FA code via {Type}", type);
            return ServiceResult.Failure($"Failed to send code via {type}");
        }
    }

    private static string FormatSecretForManualEntry(string secret)
    {
        // Format the secret in groups of 4 characters for easier manual entry
        var formatted = new StringBuilder();
        for (int i = 0; i < secret.Length; i += 4)
        {
            if (i > 0) formatted.Append(' ');
            formatted.Append(secret.Substring(i, Math.Min(4, secret.Length - i)));
        }
        return formatted.ToString();
    }
}

// Helper class for Base32 encoding (needed for TOTP)
public static class Convert
{
    private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public static string ToBase32String(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return string.Empty;

        var result = new StringBuilder();
        int index = 0;
        int digit = 0;
        int currentByte, nextByte;

        while (index < bytes.Length)
        {
            currentByte = bytes[index];

            if (digit > 3)
            {
                nextByte = index + 1 < bytes.Length ? bytes[index + 1] : 0;
                digit -= 8;
                currentByte = (currentByte << (8 - digit)) | (nextByte >> digit);
                index++;
            }

            result.Append(Base32Chars[currentByte & 31]);
            digit += 5;
        }

        return result.ToString();
    }
}
