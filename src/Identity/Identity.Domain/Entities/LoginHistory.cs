using System.ComponentModel.DataAnnotations;

namespace Identity.Domain.Entities;

public class LoginHistory
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(45)]
    public string IpAddress { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    [MaxLength(50)]
    public string? Platform { get; set; }
    
    [MaxLength(50)]
    public string? Browser { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    public DateTime LoginTime { get; set; } = DateTime.UtcNow;
    public DateTime? LogoutTime { get; set; }
    
    public bool IsSuccessful { get; set; } = true;
    
    [MaxLength(255)]
    public string? FailureReason { get; set; }
    
    public int? DeviceId { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Device? Device { get; set; }
}
