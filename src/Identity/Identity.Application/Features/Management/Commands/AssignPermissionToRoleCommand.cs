using MediatR;
using Identity.Application.Common.Models;

namespace Identity.Application.Features.Management.Commands
{
    public class AssignPermissionToRoleCommand : IRequest<ApiResponse<string>>
    {
        public string? RoleName { get; set; }
        public string? PermissionName { get; set; }
    }
}
