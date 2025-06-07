using System.ComponentModel.DataAnnotations;

namespace Identity.Domain.Entities;

public class TwoFactorToken
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(10)]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Type { get; set; } = string.Empty; // Email, SMS
    
    [MaxLength(255)]
    public string? Recipient { get; set; } // Email address or phone number
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    
    public bool IsUsed { get; set; } = false;
    public int AttemptCount { get; set; } = 0;
    
    [MaxLength(45)]
    public string? IpAddress { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
}
