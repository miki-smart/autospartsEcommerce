using MediatR;
using Identity.Application.Common.Models;
using Identity.Application.Common.DTOs;

namespace Identity.Application.Features.Users.Commands;

public class CreateUserCommand : IRequest<ApiResponse<UserDto>>
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class UpdateUserCommand : IRequest<ApiResponse<UserDto>>
{
    public string Id { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class DeleteUserCommand : IRequest<ApiResponse<bool>>
{
    public string Id { get; set; } = string.Empty;
}

public class ActivateUserCommand : IRequest<ApiResponse<bool>>
{
    public string Id { get; set; } = string.Empty;
}

public class DeactivateUserCommand : IRequest<ApiResponse<bool>>
{
    public string Id { get; set; } = string.Empty;
}

public class AssignRolesToUserCommand : IRequest<ApiResponse<bool>>
{
    public string UserId { get; set; } = string.Empty;
    public List<string> RoleIds { get; set; } = new();
}
