using MediatR;
using Identity.Application.Common.Models;
using Identity.Application.Common.DTOs;

namespace Identity.Application.Features.Roles.Commands;

public class CreateRoleCommand : IRequest<ApiResponse<RoleDto>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

public class UpdateRoleCommand : IRequest<ApiResponse<RoleDto>>
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

public class DeleteRoleCommand : IRequest<ApiResponse<bool>>
{
    public string Id { get; set; } = string.Empty;
}

public class AssignPermissionsToRoleCommand : IRequest<ApiResponse<bool>>
{
    public string RoleId { get; set; } = string.Empty;
    public List<int> PermissionIds { get; set; } = new();
}
