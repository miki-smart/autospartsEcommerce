using System.ComponentModel.DataAnnotations;

namespace Identity.Domain.Entities;

public class Device
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string DeviceIdentifier { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? DeviceName { get; set; }
    
    [MaxLength(50)]
    public string? Platform { get; set; }
    
    [MaxLength(50)]
    public string? DeviceType { get; set; } // Mobile, Desktop, Tablet
    
    [MaxLength(100)]
    public string? OperatingSystem { get; set; }
    
    [MaxLength(50)]
    public string? Browser { get; set; }
    
    public bool IsTrusted { get; set; } = false;
    public DateTime FirstSeenDate { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenDate { get; set; } = DateTime.UtcNow;
    
    [MaxLength(45)]
    public string? LastIpAddress { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();
}
