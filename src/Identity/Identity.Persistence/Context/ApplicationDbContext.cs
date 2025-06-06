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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser entity
        builder.Entity<ApplicationUser>(entity =>
        {
            // Index on IsActive for filtering active users efficiently
            entity.HasIndex(e => e.IsActive)
                  .HasDatabaseName("IX_ApplicationUser_IsActive");
            
            // Index on CreatedDate for reporting and analytics queries
            entity.HasIndex(e => e.CreatedDate)
                  .HasDatabaseName("IX_ApplicationUser_CreatedDate");

            // Configure string lengths to prevent issues and improve performance
            entity.Property(e => e.FirstName)
                  .HasMaxLength(100);
            
            entity.Property(e => e.LastName)
                  .HasMaxLength(100);
        });

        // Configure ApplicationRole entity
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.HasIndex(e => e.IsActive)
                  .HasDatabaseName("IX_ApplicationRole_IsActive");
            
            entity.Property(e => e.Description)
                  .HasMaxLength(500);
        });

        // Configure ASP.NET Core Identity table names to be more readable
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("UserTokens");
    }
}
