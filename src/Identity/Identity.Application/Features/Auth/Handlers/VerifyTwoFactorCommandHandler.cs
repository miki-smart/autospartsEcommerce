using Identity.Application.Common.DTOs;
using Identity.Application.Common.Models;
using Identity.Application.Features.Auth.Commands;
using Identity.Application.Services;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Auth.Handlers;

public class VerifyTwoFactorCommandHandler : IRequestHandler<VerifyTwoFactorCommand, ApiResponse<LoginResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITwoFactorService _twoFactorService;
    private readonly ITokenService _tokenService;
    private readonly IDeviceTrackingService _deviceTrackingService;
    private readonly ILogger<VerifyTwoFactorCommandHandler> _logger;

    public VerifyTwoFactorCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITwoFactorService twoFactorService,
        ITokenService tokenService,
        IDeviceTrackingService deviceTrackingService,
        ILogger<VerifyTwoFactorCommandHandler> logger)
    {
        _userManager = userManager;
        _twoFactorService = twoFactorService;
        _tokenService = tokenService;
        _deviceTrackingService = deviceTrackingService;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponseDto>> Handle(VerifyTwoFactorCommand request, CancellationToken cancellationToken)
    {
        try
        {            // Find the user
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("2FA verification attempted for invalid user {UserId}", request.UserId);
                return ApiResponse<LoginResponseDto>.CreateError("Invalid user");
            }

            // Parse the 2FA type
            if (!Enum.TryParse<TwoFactorType>(request.Type, out var twoFactorType))
            {
                _logger.LogWarning("Invalid 2FA type {Type} for user {UserId}", request.Type, request.UserId);
                return ApiResponse<LoginResponseDto>.CreateError("Invalid 2FA type");
            }

            // Verify the 2FA code
            ServiceResult<bool> verificationResult;

            if (twoFactorType == TwoFactorType.Authenticator)
            {
                verificationResult = await _twoFactorService.VerifyAuthenticatorCodeAsync(user, request.Token);
            }
            else
            {
                verificationResult = await _twoFactorService.VerifyCodeAsync(user.Id, request.Token, twoFactorType);
            }            if (!verificationResult.IsSuccess || !verificationResult.Data)
            {
                _logger.LogWarning("2FA verification failed for user {UserId} with type {Type}", request.UserId, twoFactorType);
                return ApiResponse<LoginResponseDto>.CreateError(
                    verificationResult.ErrorMessage ?? "Invalid verification code");
            }            // Generate JWT tokens
            var deviceInfo = new DeviceInfo
            {
                DeviceId = Guid.NewGuid().ToString(),
                IpAddress = request.IpAddress ?? "",
                UserAgent = request.UserAgent ?? "",
                Platform = null,
                Browser = null,
                OS = null,
                IsTrusted = false
            };

            var tokenResult = await _tokenService.GenerateTokenAsync(user, deviceInfo);
            if (tokenResult == null)
            {
                _logger.LogError("Token generation failed for user {UserId} after 2FA verification", request.UserId);
                return ApiResponse<LoginResponseDto>.CreateError("Authentication failed");
            }

            // Track device and login
            await _deviceTrackingService.LogDeviceAccessAsync(user.Id, deviceInfo, true);

            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("2FA verification successful for user {UserId}", request.UserId);            var response = new LoginResponseDto
            {
                AccessToken = tokenResult.AccessToken,
                RefreshToken = tokenResult.RefreshToken,
                ExpiresAt = tokenResult.AccessTokenExpiration,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    TwoFactorEnabled = user.TwoFactorEnabled
                },
                RequiresTwoFactor = false
            };

            return new ApiResponse<LoginResponseDto>(response) { Message = "2FA verification successful" };
        }        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during 2FA verification for user {UserId}", request.UserId);
            return ApiResponse<LoginResponseDto>.CreateError("2FA verification failed");
        }
    }
}
