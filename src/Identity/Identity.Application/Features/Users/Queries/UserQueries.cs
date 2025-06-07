using MediatR;
using Identity.Application.Common.Models;
using Identity.Application.Common.DTOs;

namespace Identity.Application.Features.Users.Queries;

public class GetUserByIdQuery : IRequest<ApiResponse<UserDto>>
{
    public string Id { get; set; } = string.Empty;
}

public class GetUserByEmailQuery : IRequest<ApiResponse<UserDto>>
{
    public string Email { get; set; } = string.Empty;
}

public class GetUsersQuery : IRequest<ApiResponse<List<UserDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
}

public class GetUserPermissionsQuery : IRequest<ApiResponse<List<string>>>
{
    public string UserId { get; set; } = string.Empty;
}

public class ValidateUserPermissionQuery : IRequest<ApiResponse<bool>>
{
    public string UserId { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
}
