using Identity.Application.Common.Models;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Application.Features.Management.Commands
{
    public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand, ApiResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        
        public AssignRoleToUserCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        
        public async Task<ApiResponse<string>> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
        {
            // Validate input
            if (string.IsNullOrEmpty(request.UserId))
                return ApiResponse<string>.CreateError("User ID is required");
            
            if (string.IsNullOrEmpty(request.RoleName))
                return ApiResponse<string>.CreateError("Role name is required");
            
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return ApiResponse<string>.CreateError("User not found");
            
            var result = await _userManager.AddToRoleAsync(user, request.RoleName);
            if (!result.Succeeded)
                return ApiResponse<string>.CreateError(string.Join(", ", result.Errors.Select(e => e.Description)));
            
            return new ApiResponse<string>(user.Id) { Message = "Role assigned to user successfully" };
        }
    }
}
