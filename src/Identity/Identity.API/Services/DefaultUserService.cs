using Identity.Domain.Entities;
using Identity.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Identity.Persistence.Context;

namespace Identity.API.Services;

/// <summary>
/// Service to handle creation of default users and roles at application startup
/// </summary>
public class DefaultUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DefaultUserService> _logger;

    public DefaultUserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ApplicationDbContext context,
        ILogger<DefaultUserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Creates default roles and admin user if they don't exist
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Create default roles
            await CreateDefaultRolesAsync();

            // Create default admin user
            await CreateDefaultAdminUserAsync();

            _logger.LogInformation("Default user initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while initializing default users");
            throw;
        }
    }

    private async Task CreateDefaultRolesAsync()
    {
        var defaultRoles = new[]
        {
            Roles.Admin,
            Roles.Customer,
            Roles.Mechanic,
            Roles.Vendor,
            Roles.Marketer,
            Roles.Finance,
            Roles.CustomerSupport,
            Roles.Delivery,
            Roles.DeliveryManager
        };

        foreach (var roleName in defaultRoles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var role = new ApplicationRole
                {
                    Name = roleName,
                    Description = GetRoleDescription(roleName),
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsSystemRole = true
                };

                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created role: {RoleName}", roleName);
                }
                else
                {
                    _logger.LogError("Failed to create role {RoleName}: {Errors}", 
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private async Task CreateDefaultAdminUserAsync()
    {
        const string adminEmail = "admin@autoparts.com";
        const string adminPassword = "Admin123!";

        var existingUser = await _userManager.FindByEmailAsync(adminEmail);
        if (existingUser != null)
        {
            _logger.LogInformation("Default admin user already exists");
            return;
        }

        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Administrator",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            PhoneNumberConfirmed = true,
            TwoFactorEnabled = false,
            LockoutEnabled = false
        };

        var result = await _userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            // Add to Admin role
            await _userManager.AddToRoleAsync(adminUser, Roles.Admin);
            _logger.LogInformation("Created default admin user: {Email}", adminEmail);
        }
        else
        {
            _logger.LogError("Failed to create default admin user: {Errors}", 
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private static string GetRoleDescription(string roleName)
    {
        return roleName switch
        {
            Roles.Admin => "System administrator with full access to all features",
            Roles.Customer => "Customer who purchases auto parts from the platform",
            Roles.Mechanic => "Mechanic who does affiliate marketing and can also be a customer",
            Roles.Vendor => "Vendor who lists auto parts in the platform",
            Roles.Marketer => "Manages featured items, discounts, and overall parts promotions",
            Roles.Finance => "Handles finance operations, accounts payable and receivable",
            Roles.CustomerSupport => "Provides customer support and handles complaints",
            Roles.Delivery => "Handles order delivery to customers",
            Roles.DeliveryManager => "Manages overall delivery and logistics operations",
            _ => $"{roleName} role"
        };
    }
}
