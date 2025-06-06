using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Identity.Application.Features.Auth.Commands;
using Identity.Application.Common.Models;
using Identity.Application.Common.DTOs;
using Identity.Domain.Entities;
using Identity.Application.Services;

namespace Identity.Application.Features.Auth.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<LoginResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly ITokenService _tokenService;
    private readonly IDeviceTrackingService _deviceTrackingService;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<RegisterCommandHandler> logger,
        ITokenService tokenService,
        IDeviceTrackingService deviceTrackingService)
    {
        _userManager = userManager;
        _logger = logger;
        _tokenService = tokenService;
        _deviceTrackingService = deviceTrackingService;
    }

    public async Task<ApiResponse<LoginResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing registration request for email: {Email}", request.Email);

            // Validate passwords match
            if (request.Password != request.ConfirmPassword)
            {
                _logger.LogWarning("Password confirmation mismatch for email: {Email}", request.Email);
                return new ApiResponse<LoginResponseDto>("Passwords do not match");
            }

            // Validate terms acceptance
            if (!request.AcceptTerms)
            {
                _logger.LogWarning("Terms not accepted for email: {Email}", request.Email);
                return new ApiResponse<LoginResponseDto>("You must accept the terms and conditions");
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                return new ApiResponse<LoginResponseDto>("A user with this email already exists");
            }

            // Create new user
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = false, // Will be confirmed via email verification
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                TwoFactorEnabled = false
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogError("User creation failed for email {Email}: {Errors}", request.Email, errors);
                return new ApiResponse<LoginResponseDto>(errors);
            }

            _logger.LogInformation("User created successfully for email: {Email} with ID: {UserId}", request.Email, user.Id);
            
            // Assign default Customer role
            var roleResult = await _userManager.AddToRoleAsync(user, Identity.Domain.Constants.Roles.Customer);
            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("Failed to assign Customer role to user {UserId}: {Errors}", 
                    user.Id, string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }

            // Track device information and generate tokens
            var deviceInfo = await _deviceTrackingService.CreateDeviceInfoAsync(
                request.DeviceId ?? Guid.NewGuid().ToString(),
                request.IpAddress ?? "Unknown",
                request.UserAgent ?? "Unknown",
                request.Platform,
                request.Browser,
                request.OS
            );

            // Check if this is a new device (for registration, it will always be new)
            var isNewDevice = await _deviceTrackingService.IsNewDeviceAsync(user.Id, deviceInfo.DeviceId);

            // Generate JWT tokens
            var tokenResult = await _tokenService.GenerateTokenAsync(user, deviceInfo);

            // Log device access for registration
            await _deviceTrackingService.LogDeviceAccessAsync(user.Id, deviceInfo, true);

            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);            var response = new LoginResponseDto
            {
                Success = true,
                Message = "Registration successful! Please check your email to verify your account.",
                AccessToken = tokenResult.AccessToken,
                RefreshToken = tokenResult.RefreshToken,
                ExpiresAt = tokenResult.AccessTokenExpiration,
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
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    IsActive = user.IsActive,
                    CreatedDate = user.CreatedDate,
                    LastLoginDate = user.LastLoginDate,
                    Roles = new List<string> { Identity.Domain.Constants.Roles.Customer }
                },
                RequiresTwoFactor = false,
                IsNewDevice = isNewDevice
            };

            _logger.LogInformation("Registration completed successfully for user {UserId}", user.Id);
            return new ApiResponse<LoginResponseDto>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during registration for email: {Email}", request.Email);
            return new ApiResponse<LoginResponseDto>("An error occurred during registration. Please try again.");
        }
    }
}
