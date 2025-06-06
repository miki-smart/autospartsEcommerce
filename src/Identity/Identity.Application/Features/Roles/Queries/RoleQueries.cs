using MediatR;
using Identity.Application.Common.Models;
using Identity.Application.Common.DTOs;

namespace Identity.Application.Features.Roles.Queries;

public class GetRolesQuery : IRequest<ApiResponse<List<RoleDto>>>
{
    public bool? IsActive { get; set; }
    public string? SearchTerm { get; set; }
}

public class GetRoleByIdQuery : IRequest<ApiResponse<RoleDto>>
{
    public string Id { get; set; } = string.Empty;
}

public class GetPermissionsQuery : IRequest<ApiResponse<List<PermissionDto>>>
{
    public string? Category { get; set; }
}

public class GetRolePermissionsQuery : IRequest<ApiResponse<List<PermissionDto>>>
{
    public string RoleId { get; set; } = string.Empty;
}
