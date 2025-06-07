using MediatR;
using Microsoft.Extensions.Logging;
using Identity.Application.Common.Models;
using Identity.Application.Common.DTOs;
using Identity.Application.Features.Auth.Commands;
using Identity.Application.Services;

namespace Identity.Application.Features.Auth.Handlers;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<LoginResponseDto>>
{
    private readonly ITokenService _tokenService;
    private readonly IDeviceTrackingService _deviceTrackingService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        ITokenService tokenService,
        IDeviceTrackingService deviceTrackingService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _tokenService = tokenService;
        _deviceTrackingService = deviceTrackingService;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Refresh token attempt for token: {Token}", request.RefreshToken[..8] + "...");

            // Create device info for refresh token request
            var deviceInfo = await _deviceTrackingService.CreateDeviceInfoAsync(
                request.DeviceId ?? Guid.NewGuid().ToString(),
                request.IpAddress ?? "Unknown",
                request.UserAgent ?? "Unknown"
            );

            // Attempt to refresh tokens
            var tokenResult = await _tokenService.RefreshTokenAsync(request.RefreshToken, deviceInfo);

            if (tokenResult == null)
            {
                _logger.LogWarning("Refresh token failed - invalid token");
                return new ApiResponse<LoginResponseDto>("Invalid refresh token");
            }

            var response = new LoginResponseDto
            {
                Success = true,
                Message = "Token refreshed successfully",
                AccessToken = tokenResult.AccessToken,
                RefreshToken = tokenResult.RefreshToken,
                ExpiresAt = tokenResult.AccessTokenExpiration
            };

            _logger.LogInformation("Token refresh successful");
            return new ApiResponse<LoginResponseDto>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return new ApiResponse<LoginResponseDto>("An error occurred during token refresh");
        }
    }
}
