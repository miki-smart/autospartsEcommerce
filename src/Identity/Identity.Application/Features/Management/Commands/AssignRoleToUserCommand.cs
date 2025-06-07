using MediatR;
using Identity.Application.Common.Models;

namespace Identity.Application.Features.Management.Commands
{
    public class AssignRoleToUserCommand : IRequest<ApiResponse<string>>
    {
        public string? UserId { get; set; }
        public string? RoleName { get; set; }
    }
}
