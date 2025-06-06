using MediatR;
using Identity.Application.Common.Models;

namespace Identity.Application.Features.Management.Commands
{
    public class CreatePermissionCommand : IRequest<ApiResponse<string>>
    {
        public string? PermissionName { get; set; }
        public string? Description { get; set; }
    }
}
