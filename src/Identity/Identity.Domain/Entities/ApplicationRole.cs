using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Identity.Domain.Entities;

public class ApplicationRole : IdentityRole
{
    [MaxLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    
    [MaxLength(450)]
    public string? CreatedBy { get; set; }
    
    [MaxLength(450)]
    public string? ModifiedBy { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool IsSystemRole { get; set; } = false;
    
    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
