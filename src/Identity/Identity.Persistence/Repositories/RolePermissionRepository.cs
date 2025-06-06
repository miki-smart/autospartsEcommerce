using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using Identity.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Repositories;

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly ApplicationDbContext _context;

    public RolePermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(string roleId)
    {
        return await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Include(rp => rp.Role)
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();
    }

    public async Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(int permissionId)
    {
        return await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Include(rp => rp.Role)
            .Where(rp => rp.PermissionId == permissionId)
            .ToListAsync();
    }

    public async Task<RolePermission?> GetByRoleAndPermissionAsync(string roleId, int permissionId)
    {
        return await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Include(rp => rp.Role)
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
    }

    public async Task<bool> ExistsAsync(string roleId, int permissionId)
    {
        return await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
    }

    public async Task<RolePermission> CreateAsync(RolePermission rolePermission)
    {
        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();
        return rolePermission;
    }

    public async Task DeleteAsync(RolePermission rolePermission)
    {
        _context.RolePermissions.Remove(rolePermission);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByRoleIdAsync(string roleId)
    {
        var rolePermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();
        
        _context.RolePermissions.RemoveRange(rolePermissions);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByPermissionIdAsync(int permissionId)
    {
        var rolePermissions = await _context.RolePermissions
            .Where(rp => rp.PermissionId == permissionId)
            .ToListAsync();
        
        _context.RolePermissions.RemoveRange(rolePermissions);
        await _context.SaveChangesAsync();
    }
}
