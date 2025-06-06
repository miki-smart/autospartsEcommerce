using Identity.Domain.Entities;

namespace Identity.Domain.Repositories;

public interface IRolePermissionRepository
{
    Task<IEnumerable<RolePermission>> GetByRoleIdAsync(string roleId);
    Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(int permissionId);
    Task<RolePermission?> GetByRoleAndPermissionAsync(string roleId, int permissionId);
    Task<bool> ExistsAsync(string roleId, int permissionId);
    Task<RolePermission> CreateAsync(RolePermission rolePermission);
    Task DeleteAsync(RolePermission rolePermission);
    Task DeleteByRoleIdAsync(string roleId);
    Task DeleteByPermissionIdAsync(int permissionId);
}
