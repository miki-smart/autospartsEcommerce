using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Context;

/// <summary>
/// Main database context for Identity Server
/// Inherits from IdentityDbContext to get ASP.NET Core Identity tables
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
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
        });

        // Configure ASP.NET Core Identity table names
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("UserTokens");
        
        // Seed initial data
        SeedData(builder);
    }

    private static void SeedData(ModelBuilder builder)
    {
        // Seed Permissions
        var permissions = new List<Permission>();
        var permissionId = 1;
        
        // Add all permissions from Constants
        foreach (var permission in Identity.Domain.Constants.Permissions.AllPermissions)
        {
            var category = GetPermissionCategory(permission);
            permissions.Add(new Permission
            {
                Id = permissionId++,
                Name = permission,
                Description = GetPermissionDescription(permission),
                Category = category,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            });
        }
        
        builder.Entity<Permission>().HasData(permissions);
        
        // Seed Roles
        var roles = new List<ApplicationRole>();
        foreach (var roleName in Identity.Domain.Constants.Roles.AllRoles)
        {
            roles.Add(new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = roleName,
                NormalizedName = roleName.ToUpper(),
                Description = GetRoleDescription(roleName),
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsSystemRole = true,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });
        }
        
        builder.Entity<ApplicationRole>().HasData(roles);
        
        // Seed RolePermissions based on DefaultRolePermissions mapping
        var rolePermissions = new List<RolePermission>();
        var rolePermissionId = 1;
        
        foreach (var role in roles)
        {
            var rolePermissionNames = Identity.Domain.Configuration.DefaultRolePermissions.GetPermissionsForRole(role.Name!);
            foreach (var permissionName in rolePermissionNames)
            {
                var permission = permissions.FirstOrDefault(p => p.Name == permissionName);
                if (permission != null)
                {
                    rolePermissions.Add(new RolePermission
                    {
                        Id = rolePermissionId++,
                        RoleId = role.Id,
                        PermissionId = permission.Id,
                        GrantedDate = DateTime.UtcNow,
                        GrantedBy = "System"
                    });
                }
            }
        }
        
        builder.Entity<RolePermission>().HasData(rolePermissions);
    }
    
    private static string GetPermissionCategory(string permission)
    {
        return permission.Split('.')[0] switch
        {
            "users" => Identity.Domain.Constants.PermissionCategories.UserManagement,
            "roles" => Identity.Domain.Constants.PermissionCategories.RoleManagement,
            "permissions" => Identity.Domain.Constants.PermissionCategories.PermissionManagement,
            "parts" => Identity.Domain.Constants.PermissionCategories.PartsManagement,
            "inventory" => Identity.Domain.Constants.PermissionCategories.InventoryManagement,
            "orders" => Identity.Domain.Constants.PermissionCategories.OrderManagement,
            "marketing" or "discounts" or "featured-items" or "campaigns" => Identity.Domain.Constants.PermissionCategories.MarketingPromotions,
            "finance" or "accounts" or "payments" or "invoices" or "refunds" => Identity.Domain.Constants.PermissionCategories.FinanceAccounting,
            "support" or "tickets" or "customer" or "complaints" => Identity.Domain.Constants.PermissionCategories.CustomerSupport,
            "delivery" or "logistics" or "route" => Identity.Domain.Constants.PermissionCategories.DeliveryLogistics,
            "vendors" or "vendor" => Identity.Domain.Constants.PermissionCategories.VendorManagement,
            "affiliate" or "commissions" => Identity.Domain.Constants.PermissionCategories.AffiliateManagement,
            "reports" or "analytics" or "dashboard" => Identity.Domain.Constants.PermissionCategories.Reporting,
            "system" or "audit-logs" => Identity.Domain.Constants.PermissionCategories.SystemAdministration,
            _ => "General"
        };
    }
    
    private static string GetPermissionDescription(string permission)
    {
        var parts = permission.Split('.');
        var action = parts.Length > 1 ? parts[1] : parts[0];
        var resource = parts[0];
        
        return action switch
        {
            "read" => $"View {resource}",
            "create" => $"Create {resource}",
            "update" => $"Update {resource}",
            "delete" => $"Delete {resource}",
            "manage" => $"Full management of {resource}",
            "assign" => $"Assign {resource}",
            "approve" => $"Approve {resource}",
            "process" => $"Process {resource}",
            "export" => $"Export {resource}",
            "tracking" => $"Track {resource}",
            "optimization" => $"Optimize {resource}",
            "onboarding" => $"Onboard {resource}",
            "interact" => $"Interact with {resource}",
            "handle" => $"Handle {resource}",
            "on-behalf" => $"Act on behalf for {resource}",
            "access" => $"Access {resource}",
            _ => $"Perform {action} on {resource}"
        };
    }
    
    private static string GetRoleDescription(string roleName)
    {
        return roleName switch
        {
            Identity.Domain.Constants.Roles.Admin => "System administrator with full access to all features",
            Identity.Domain.Constants.Roles.Customer => "Customer who purchases auto parts from the platform",
            Identity.Domain.Constants.Roles.Mechanic => "Mechanic who does affiliate marketing and can also be a customer",
            Identity.Domain.Constants.Roles.Vendor => "Vendor who lists auto parts in the platform",
            Identity.Domain.Constants.Roles.Marketer => "Manages featured items, discounts, and overall parts promotions",
            Identity.Domain.Constants.Roles.Finance => "Handles finance operations, accounts payable and receivable",
            Identity.Domain.Constants.Roles.CustomerSupport => "Provides customer support and handles complaints",
            Identity.Domain.Constants.Roles.Delivery => "Handles order delivery to customers",
            Identity.Domain.Constants.Roles.DeliveryManager => "Manages overall delivery and logistics operations",
            _ => $"{roleName} role"
        };
    }
}
