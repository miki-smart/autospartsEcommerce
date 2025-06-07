using MediatR;
using Identity.Application.Common.Models;
using Identity.Application.Common.DTOs;

namespace Identity.Application.Features.Auth.Commands;

public class LoginCommand : IRequest<ApiResponse<LoginResponseDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
    public string? DeviceId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Platform { get; set; }
    public string? Browser { get; set; }
    public string? OS { get; set; }
}

public class RefreshTokenCommand : IRequest<ApiResponse<LoginResponseDto>>
{
    public string RefreshToken { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class LogoutCommand : IRequest<ApiResponse<bool>>
{
    public string UserId { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
}

public class VerifyTwoFactorCommand : IRequest<ApiResponse<LoginResponseDto>>
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class SendTwoFactorTokenCommand : IRequest<ApiResponse<bool>>
{
    public string UserId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Email, SMS
    public string? IpAddress { get; set; }
}

public class ResetPasswordCommand : IRequest<ApiResponse<bool>>
{
    public string Email { get; set; } = string.Empty;
}

public class ConfirmResetPasswordCommand : IRequest<ApiResponse<bool>>
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordCommand : IRequest<ApiResponse<bool>>
{
    public string UserId { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class RegisterCommand : IRequest<ApiResponse<LoginResponseDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? DeviceId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Platform { get; set; }
    public string? Browser { get; set; }
    public string? OS { get; set; }
    public bool AcceptTerms { get; set; } = false;
}

public class ForgotPasswordCommand : IRequest<ApiResponse<bool>>
{
    public string Email { get; set; } = string.Empty;
}
