using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Identity.Application.Common.Models;
using Identity.Application.Common.DTOs;
using Identity.Application.Features.Auth.Commands;
using Identity.Application.Services;
using Identity.Domain.Entities;
using Identity.Domain.Constants;

namespace Identity.Application.Features.Auth.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IDeviceTrackingService _deviceTrackingService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IDeviceTrackingService deviceTrackingService,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _deviceTrackingService = deviceTrackingService;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed - user not found: {Email}", request.Email);
                return new ApiResponse<LoginResponseDto>("Invalid credentials");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed - account inactive: {UserId}", user.Id);
                return new ApiResponse<LoginResponseDto>("Account is inactive");
            }

            // if (!user.EmailConfirmed)
            // {
            //     _logger.LogWarning("Login failed - email not confirmed: {UserId}", user.Id);
            //     return new ApiResponse<LoginResponseDto>("Email not confirmed");
            // }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Login failed - account locked: {UserId}", user.Id);
                return new ApiResponse<LoginResponseDto>("Account is locked");
            }

            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed - invalid password: {UserId}", user.Id);
                return new ApiResponse<LoginResponseDto>("Invalid credentials");
            }

            // Check if 2FA is required
            if (result.RequiresTwoFactor)
            {
                _logger.LogInformation("2FA required for user: {UserId}", user.Id);
                // TODO: Generate and send 2FA token
                return new ApiResponse<LoginResponseDto>(new LoginResponseDto
                {
                    Success = true,
                    RequiresTwoFactor = true,
                    Message = "Two-factor authentication required"
                });
            }            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Create device info
            var deviceInfo = await _deviceTrackingService.CreateDeviceInfoAsync(
                request.DeviceId ?? Guid.NewGuid().ToString(),
                request.IpAddress ?? "Unknown",
                request.UserAgent ?? "Unknown",
                request.Platform,
                request.Browser,
                request.OS
            );

            // Check if this is a new device
            var isNewDevice = await _deviceTrackingService.IsNewDeviceAsync(user.Id, deviceInfo.DeviceId);

            // Generate JWT tokens
            var tokenResult = await _tokenService.GenerateTokenAsync(user, deviceInfo);

            // Log device access
            await _deviceTrackingService.LogDeviceAccessAsync(user.Id, deviceInfo, true);

            var userRoles = await _userManager.GetRolesAsync(user);
              var loginResponse = new LoginResponseDto
            {
                Success = true,
                Message = "Login successful",
                AccessToken = tokenResult.AccessToken,
                RefreshToken = tokenResult.RefreshToken,
                ExpiresAt = tokenResult.AccessTokenExpiration,
                IsNewDevice = isNewDevice,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    IsActive = user.IsActive,
                    CreatedDate = user.CreatedDate,
                    LastLoginDate = user.LastLoginDate,
                    Roles = userRoles.ToList()
                }
            };

            _logger.LogInformation("Login successful for user: {UserId}", user.Id);
            return new ApiResponse<LoginResponseDto>(loginResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return new ApiResponse<LoginResponseDto>("An error occurred during login");
        }
    }
}
