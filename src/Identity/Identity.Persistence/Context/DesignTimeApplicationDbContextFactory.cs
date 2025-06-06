using Identity.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Identity.Persistence.Context;

public class DesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Find the root path and load configuration
        var basePath = Directory.GetCurrentDirectory();
        
        // Look for appsettings.json in the API project
        var apiBasePath = Path.Combine(basePath, "..", "Identity.API");
        if (!Directory.Exists(apiBasePath))
        {
            // If we're running from API project directory
            apiBasePath = basePath;
        }
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiBasePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=IdentityDb;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
