using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Identity.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginDate { get; set; }
    public DateTime? LastPasswordChangeDate { get; set; }
      public bool IsActive { get; set; } = true;
    public new bool TwoFactorEnabled { get; set; } = false;
    
    [MaxLength(255)]
    public string? ProfilePicture { get; set; }
    
    [MaxLength(50)]
    public string? PreferredLanguage { get; set; } = "en";
    
    [MaxLength(50)]
    public string? TimeZone { get; set; }
    
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    
    // Full name property
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    // Navigation properties
    public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();
    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
    public virtual ICollection<TwoFactorToken> TwoFactorTokens { get; set; } = new List<TwoFactorToken>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
