using Identity.Domain.Entities;
using Identity.Infrastructure.Config;
using Identity.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add IdentityServer
        services.AddIdentityServer(options =>
        {
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;

            // See https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
            options.EmitStaticAudienceClaim = true;
        })        .AddInMemoryIdentityResources(Identity.Infrastructure.Config.Config.IdentityResources)
        .AddInMemoryApiScopes(Identity.Infrastructure.Config.Config.ApiScopes)
        .AddInMemoryClients(Identity.Infrastructure.Config.Config.Clients)
        .AddAspNetIdentity<ApplicationUser>()
        .AddConfigurationStore(options =>
        {
            options.ConfigureDbContext = b => b.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        })
        .AddOperationalStore(options =>
        {
            options.ConfigureDbContext = b => b.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        })
        .AddDeveloperSigningCredential(); // Not recommended for production - use AddSigningCredential() instead

        return services;
    }
}
