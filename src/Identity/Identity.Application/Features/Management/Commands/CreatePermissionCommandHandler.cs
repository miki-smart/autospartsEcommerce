using Identity.Application.Common.Models;
using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Application.Features.Management.Commands
{
    public class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, ApiResponse<string>>
    {
        private readonly IPermissionRepository _permissionRepository;
        
        public CreatePermissionCommandHandler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }
        
        public async Task<ApiResponse<string>> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
        {
            // Validate input
            if (string.IsNullOrEmpty(request.PermissionName))
                return ApiResponse<string>.CreateError("Permission name is required");
            
            var exists = await _permissionRepository.ExistsAsync(request.PermissionName);
            if (exists)
                return ApiResponse<string>.CreateError("Permission already exists");
            
            var permission = new Permission 
            { 
                Name = request.PermissionName, 
                Description = request.Description ?? string.Empty,
                Category = "General",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            
            var createdPermission = await _permissionRepository.CreateAsync(permission);
            
            return new ApiResponse<string>(createdPermission.Name ?? "Permission created") 
            { 
                Message = "Permission created successfully" 
            };
        }
    }
}
