using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Context;

/// <summary>
/// Main database context for Identity Server
/// Inherits from IdentityDbContext to get ASP.NET Core Identity tables
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        // Suppress the non-deterministic model warning
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    // Additional DbSets for custom entities
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<LoginHistory> LoginHistories { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<TwoFactorToken> TwoFactorTokens { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser entity
        builder.Entity<ApplicationUser>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(e => e.IsActive)
                  .HasDatabaseName("IX_Users_IsActive");
            
            entity.HasIndex(e => e.CreatedDate)
                  .HasDatabaseName("IX_Users_CreatedDate");
                  
            entity.HasIndex(e => e.LastLoginDate)
                  .HasDatabaseName("IX_Users_LastLoginDate");

            // Configure string lengths
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.ProfilePicture).HasMaxLength(255);
            entity.Property(e => e.PreferredLanguage).HasMaxLength(50);
            entity.Property(e => e.TimeZone).HasMaxLength(50);
        });

        // Configure ApplicationRole entity
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.HasIndex(e => e.IsActive)
                  .HasDatabaseName("IX_Roles_IsActive");
                  
            entity.HasIndex(e => e.IsSystemRole)
                  .HasDatabaseName("IX_Roles_IsSystemRole");
            
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.CreatedBy).HasMaxLength(450);
            entity.Property(e => e.ModifiedBy).HasMaxLength(450);
        });

        // Configure Permission entity
        builder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique()
                  .HasDatabaseName("IX_Permissions_Name");
            entity.HasIndex(e => e.Category)
                  .HasDatabaseName("IX_Permissions_Category");
            entity.HasIndex(e => e.IsActive)
                  .HasDatabaseName("IX_Permissions_IsActive");
                  
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
        });

        // Configure RolePermission entity (Many-to-Many)
        builder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique()
                  .HasDatabaseName("IX_RolePermissions_RoleId_PermissionId");
                  
            entity.HasOne(rp => rp.Role)
                  .WithMany(r => r.RolePermissions)
                  .HasForeignKey(rp => rp.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(rp => rp.Permission)
                  .WithMany(p => p.RolePermissions)
                  .HasForeignKey(rp => rp.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.Property(e => e.GrantedBy).HasMaxLength(450);
        });

        // Configure Device entity
        builder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("IX_Devices_UserId");
            entity.HasIndex(e => e.DeviceIdentifier)
                  .HasDatabaseName("IX_Devices_DeviceIdentifier");
            entity.HasIndex(e => e.IsActive)
                  .HasDatabaseName("IX_Devices_IsActive");
                  
            entity.HasOne(d => d.User)
                  .WithMany(u => u.Devices)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.Property(e => e.DeviceIdentifier).HasMaxLength(255).IsRequired();
            entity.Property(e => e.DeviceName).HasMaxLength(100);
            entity.Property(e => e.Platform).HasMaxLength(50);
            entity.Property(e => e.DeviceType).HasMaxLength(50);
            entity.Property(e => e.OperatingSystem).HasMaxLength(100);
            entity.Property(e => e.Browser).HasMaxLength(50);
            entity.Property(e => e.LastIpAddress).HasMaxLength(45);
        });

        // Configure LoginHistory entity
        builder.Entity<LoginHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("IX_LoginHistory_UserId");
            entity.HasIndex(e => e.LoginTime)
                  .HasDatabaseName("IX_LoginHistory_LoginTime");
            entity.HasIndex(e => e.IpAddress)
                  .HasDatabaseName("IX_LoginHistory_IpAddress");
            entity.HasIndex(e => e.IsSuccessful)
                  .HasDatabaseName("IX_LoginHistory_IsSuccessful");
                  
            entity.HasOne(lh => lh.User)
                  .WithMany(u => u.LoginHistories)
                  .HasForeignKey(lh => lh.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(lh => lh.Device)
                  .WithMany(d => d.LoginHistories)
                  .HasForeignKey(lh => lh.DeviceId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.Property(e => e.IpAddress).HasMaxLength(45).IsRequired();
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Platform).HasMaxLength(50);
            entity.Property(e => e.Browser).HasMaxLength(50);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.FailureReason).HasMaxLength(255);
        });

        // Configure TwoFactorToken entity
        builder.Entity<TwoFactorToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("IX_TwoFactorTokens_UserId");
            entity.HasIndex(e => e.Token)
                  .HasDatabaseName("IX_TwoFactorTokens_Token");
            entity.HasIndex(e => e.ExpiresAt)
                  .HasDatabaseName("IX_TwoFactorTokens_ExpiresAt");
            entity.HasIndex(e => e.IsUsed)
                  .HasDatabaseName("IX_TwoFactorTokens_IsUsed");
                  
            entity.HasOne(tft => tft.User)
                  .WithMany(u => u.TwoFactorTokens)
                  .HasForeignKey(tft => tft.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.Property(e => e.Token).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Recipient).HasMaxLength(255);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
        });

        // Configure RefreshToken entity
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("IX_RefreshTokens_UserId");
            entity.HasIndex(e => e.Token).IsUnique()
                  .HasDatabaseName("IX_RefreshTokens_Token");
            entity.HasIndex(e => e.JwtId)
                  .HasDatabaseName("IX_RefreshTokens_JwtId");
            entity.HasIndex(e => e.ExpiresAt)
                  .HasDatabaseName("IX_RefreshTokens_ExpiresAt");
            entity.HasIndex(e => e.IsRevoked)
                  .HasDatabaseName("IX_RefreshTokens_IsRevoked");
                  
            entity.HasOne(rt => rt.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(rt => rt.Device)
                  .WithMany()
                  .HasForeignKey(rt => rt.DeviceId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.Property(e => e.Token).HasMaxLength(255).IsRequired();
            entity.Property(e => e.JwtId).HasMaxLength(255).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
        });        // Configure ASP.NET Core Identity table names
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("UserTokens");
        
        // Seed data removed to prevent migration issues - will be handled at application startup
    }
}
