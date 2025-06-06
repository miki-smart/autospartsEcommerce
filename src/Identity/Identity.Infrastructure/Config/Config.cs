using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Identity.Infrastructure.Config;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("catalog.read", "Read access to catalog service"),
            new ApiScope("catalog.write", "Write access to catalog service"),
            new ApiScope("orders.read", "Read access to orders service"),
            new ApiScope("orders.write", "Write access to orders service"),
            new ApiScope("basket.read", "Read access to basket service"),
            new ApiScope("basket.write", "Write access to basket service"),
            new ApiScope("payment.read", "Read access to payment service"),
            new ApiScope("payment.write", "Write access to payment service"),
            new ApiScope("search.read", "Read access to search service"),
            new ApiScope("notification.read", "Read access to notification service"),
            new ApiScope("notification.write", "Write access to notification service"),
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // Machine to machine client (Client Credentials Flow)
            new Client
            {
                ClientId = "autoparts.services",
                ClientName = "AutoParts Services",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("autoparts-secret".Sha256()) },
                AllowedScopes = { 
                    "catalog.read", "catalog.write",
                    "orders.read", "orders.write",
                    "basket.read", "basket.write",
                    "payment.read", "payment.write",
                    "search.read", 
                    "notification.read", "notification.write"
                }
            },

            // Interactive ASP.NET Core Web App
            new Client
            {
                ClientId = "autoparts.web",
                ClientName = "AutoParts Web Application",
                ClientSecrets = { new Secret("autoparts-web-secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                // Where to redirect to after login
                RedirectUris = { "https://localhost:5001/signin-oidc" },

                // Where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "catalog.read", "catalog.write",
                    "orders.read", "orders.write",
                    "basket.read", "basket.write",
                    "payment.read", "payment.write",
                    "search.read",
                    "notification.read", "notification.write"
                }
            },

            // JavaScript Client (SPA)
            new Client
            {
                ClientId = "autoparts.spa",
                ClientName = "AutoParts SPA",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,

                RedirectUris = 
                { 
                    "http://localhost:3000/callback",
                    "https://localhost:3000/callback"
                },

                PostLogoutRedirectUris = 
                { 
                    "http://localhost:3000/",
                    "https://localhost:3000/"
                },

                AllowedCorsOrigins = 
                { 
                    "http://localhost:3000",
                    "https://localhost:3000"
                },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "catalog.read",
                    "orders.read", "orders.write",
                    "basket.read", "basket.write",
                    "payment.read", "payment.write",
                    "search.read"
                }
            },

            // Mobile App Client
            new Client
            {
                ClientId = "autoparts.mobile",
                ClientName = "AutoParts Mobile App",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,

                RedirectUris = { "autoparts://authenticated" },
                PostLogoutRedirectUris = { "autoparts://logout" },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "catalog.read",
                    "orders.read", "orders.write",
                    "basket.read", "basket.write",
                    "payment.read", "payment.write",
                    "search.read"
                }
            }
        };
}
