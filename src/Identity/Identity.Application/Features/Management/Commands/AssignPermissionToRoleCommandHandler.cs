using Identity.Application.Common.Models;
using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Application.Features.Management.Commands
{
    public class AssignPermissionToRoleCommandHandler : IRequestHandler<AssignPermissionToRoleCommand, ApiResponse<string>>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        
        public AssignPermissionToRoleCommandHandler(
            RoleManager<ApplicationRole> roleManager,
            IPermissionRepository permissionRepository,
            IRolePermissionRepository rolePermissionRepository)
        {
            _roleManager = roleManager;
            _permissionRepository = permissionRepository;
            _rolePermissionRepository = rolePermissionRepository;
        }
        
        public async Task<ApiResponse<string>> Handle(AssignPermissionToRoleCommand request, CancellationToken cancellationToken)
        {
            // Validate input
            if (string.IsNullOrEmpty(request.RoleName))
                return ApiResponse<string>.CreateError("Role name is required");
            
            if (string.IsNullOrEmpty(request.PermissionName))
                return ApiResponse<string>.CreateError("Permission name is required");
            
            var role = await _roleManager.FindByNameAsync(request.RoleName);
            if (role == null)
                return ApiResponse<string>.CreateError("Role not found");
            
            var permission = await _permissionRepository.GetByNameAsync(request.PermissionName);
            if (permission == null)
                return ApiResponse<string>.CreateError("Permission not found");
            
            var alreadyAssigned = await _rolePermissionRepository.ExistsAsync(role.Id, permission.Id);
            if (alreadyAssigned)
                return ApiResponse<string>.CreateError("Permission already assigned to role");
            
            var rolePermission = new RolePermission 
            { 
                RoleId = role.Id, 
                PermissionId = permission.Id,
                GrantedDate = DateTime.UtcNow,
                GrantedBy = "System" // You can modify this to use current user
            };
            
            await _rolePermissionRepository.CreateAsync(rolePermission);
            
            return new ApiResponse<string>(role.Name ?? "Role") { Message = "Permission assigned to role successfully" };
        }
    }
}
