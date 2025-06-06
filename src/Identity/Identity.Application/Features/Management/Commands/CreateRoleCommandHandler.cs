using Identity.Application.Common.Models;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Application.Features.Management.Commands
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, ApiResponse<string>>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        public CreateRoleCommandHandler(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }
        public async Task<ApiResponse<string>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RoleName))
                return ApiResponse<string>.CreateError("Role name is required");
            var exists = await _roleManager.RoleExistsAsync(request.RoleName);
            if (exists)
                return ApiResponse<string>.CreateError("Role already exists");
            var result = await _roleManager.CreateAsync(new ApplicationRole { Name = request.RoleName });
            if (!result.Succeeded)
                return ApiResponse<string>.CreateError(string.Join(", ", result.Errors.Select(e => e.Description)));
            return new ApiResponse<string>(request.RoleName) { Message = "Role created successfully" };
        }
    }
}
