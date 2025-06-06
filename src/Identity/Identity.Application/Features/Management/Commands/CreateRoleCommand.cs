using MediatR;
using Identity.Application.Common.Models;

namespace Identity.Application.Features.Management.Commands
{
    public class CreateRoleCommand : IRequest<ApiResponse<string>>
    {
        public string? RoleName { get; set; }
    }
}
