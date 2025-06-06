using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Identity.Application.Features.Auth.Commands;
using Identity.Application.Common.DTOs;
using Identity.Application.Common.Models;

namespace Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>Login response with tokens and user information</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 401)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var command = new LoginCommand
            {
                Email = loginDto.Email,
                Password = loginDto.Password,
                RememberMe = loginDto.RememberMe,
                DeviceId = loginDto.DeviceInfo,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                Platform = GetPlatform(),
                Browser = GetBrowser(),
                OS = GetOperatingSystem()
            };

            var result = await _mediator.Send(command);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new ApiResponse<LoginResponseDto>("Internal server error"));
        }
    }

    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="registerDto">Registration details</param>
    /// <returns>Registration response with tokens and user information</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var command = new RegisterCommand
            {
                Email = registerDto.Email,
                Password = registerDto.Password,
                ConfirmPassword = registerDto.ConfirmPassword,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                AcceptTerms = registerDto.AcceptTerms,
                DeviceId = registerDto.DeviceInfo,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                Platform = GetPlatform(),
                Browser = GetBrowser(),
                OS = GetOperatingSystem()
            };

            var result = await _mediator.Send(command);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new ApiResponse<LoginResponseDto>("Internal server error"));
        }
    }

    /// <summary>
    /// Refreshes JWT tokens using a valid refresh token
    /// </summary>
    /// <param name="refreshTokenDto">Refresh token request</param>
    /// <returns>New tokens</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 401)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        try
        {
            var command = new RefreshTokenCommand
            {
                RefreshToken = refreshTokenDto.RefreshToken,
                DeviceId = Request.Headers["X-Device-Id"].FirstOrDefault(),
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent()
            };

            var result = await _mediator.Send(command);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new ApiResponse<LoginResponseDto>("Internal server error"));
        }
    }

    /// <summary>
    /// Logs out a user and revokes their refresh token
    /// </summary>
    /// <param name="logoutDto">Logout request</param>
    /// <returns>Logout confirmation</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<IActionResult> Logout([FromBody] LogoutDto logoutDto)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            
            var command = new LogoutCommand
            {
                UserId = userId!,
                RefreshToken = logoutDto.RefreshToken
            };

            var result = await _mediator.Send(command);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new ApiResponse<bool>("Internal server error"));
        }
    }

    /// <summary>
    /// Verifies two-factor authentication token
    /// </summary>
    /// <param name="twoFactorDto">2FA verification request</param>
    /// <returns>Login response with tokens if successful</returns>
    [HttpPost("verify-2fa")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 400)]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorDto twoFactorDto)
    {
        try
        {
            var command = new VerifyTwoFactorCommand
            {
                UserId = twoFactorDto.UserId,
                Token = twoFactorDto.Token,
                Type = twoFactorDto.Type,
                DeviceInfo = twoFactorDto.DeviceInfo,
                IpAddress = GetClientIpAddress()
            };

            var result = await _mediator.Send(command);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during 2FA verification");
            return StatusCode(500, new ApiResponse<LoginResponseDto>("Internal server error"));
        }
    }

    #region Private Helper Methods

    private string GetClientIpAddress()
    {
        var xForwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',')[0].Trim();
        }

        var xRealIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp))
        {
            return xRealIp;
        }

        return Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent()
    {
        return Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
    }

    private string? GetPlatform()
    {
        return Request.Headers["X-Platform"].FirstOrDefault();
    }

    private string? GetBrowser()
    {
        return Request.Headers["X-Browser"].FirstOrDefault();
    }

    private string? GetOperatingSystem()
    {
        return Request.Headers["X-OS"].FirstOrDefault();
    }

    #endregion
}

public class LogoutDto
{
    public string? RefreshToken { get; set; }
}
