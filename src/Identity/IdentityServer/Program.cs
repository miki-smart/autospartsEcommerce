using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/identityserver-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure IdentityServer
builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
    
    // Emit static audience claim for easier JWT validation
    options.EmitStaticAudienceClaim = true;
})
.AddInMemoryIdentityResources(GetIdentityResources())
.AddInMemoryApiScopes(GetApiScopes())
.AddInMemoryApiResources(GetApiResources())
.AddInMemoryClients(GetClients())
.AddDeveloperSigningCredential(); // Only for development

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("IdentityServer is running"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSerilogRequestLogging();

app.UseRouting();

app.UseCors("AllowAll");

app.UseIdentityServer();

// Health Check endpoint
app.MapHealthChecks("/health");

// Basic info endpoint
app.MapGet("/", () => new
{
    Service = "Duende IdentityServer",
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow,
    Endpoints = new
    {
        Discovery = "/.well-known/openid_configuration",
        Token = "/connect/token",
        UserInfo = "/connect/userinfo",
        Introspection = "/connect/introspect"
    }
});

// Identity API endpoints for user management through gateway
app.MapPost("/api/identity/register", (object registrationRequest) =>
{
    Log.Information("User registration request: {@Request}", registrationRequest);
    return Results.Ok(new { Message = "User registered successfully", UserId = Guid.NewGuid() });
});

app.MapPost("/api/identity/login", (object loginRequest) =>
{
    Log.Information("User login request: {@Request}", loginRequest);
    return Results.Ok(new { Message = "Login successful", Token = "sample-jwt-token" });
});

app.MapGet("/api/identity/user-info", (HttpContext context) =>
{
    var user = context.User;
    return Results.Ok(new 
    { 
        IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
        Name = user.Identity?.Name,
        Claims = user.Claims.Select(c => new { c.Type, c.Value })
    });
}).RequireAuthorization();

app.Run();

// IdentityServer Configuration
static IEnumerable<IdentityResource> GetIdentityResources()
{
    return new IdentityResource[]
    {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResources.Email()
    };
}

static IEnumerable<ApiScope> GetApiScopes()
{
    return new ApiScope[]
    {
        new ApiScope("catalog.read", "Read access to catalog service"),
        new ApiScope("catalog.write", "Write access to catalog service"),
        new ApiScope("users.read", "Read access to user management service"),
        new ApiScope("users.write", "Write access to user management service"),
        new ApiScope("gateway.access", "Access to API Gateway"),
        new ApiScope("admin", "Admin access to all services")
    };
}

static IEnumerable<ApiResource> GetApiResources()
{
    return new ApiResource[]
    {
        new ApiResource("catalog-api", "Catalog Service API")
        {
            Scopes = { "catalog.read", "catalog.write" }
        },
        new ApiResource("user-management-api", "User Management Service API")
        {
            Scopes = { "users.read", "users.write" }
        },
        new ApiResource("gateway-api", "API Gateway")
        {
            Scopes = { "gateway.access" }
        }
    };
}

static IEnumerable<Client> GetClients()
{
    return new Client[]
    {
        // Client credentials flow - for service-to-service communication
        new Client
        {
            ClientId = "catalog-service-client",
            ClientName = "Catalog Service Client",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("catalog-secret".Sha256()) },
            AllowedScopes = { "catalog.read", "catalog.write" }
        },
        
        new Client
        {
            ClientId = "user-management-service-client",
            ClientName = "User Management Service Client",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("user-management-secret".Sha256()) },
            AllowedScopes = { "users.read", "users.write" }
        },
        
        // API Gateway client
        new Client
        {
            ClientId = "api-gateway-client",
            ClientName = "API Gateway Client",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("gateway-secret".Sha256()) },
            AllowedScopes = { "gateway.access", "catalog.read", "catalog.write", "users.read", "users.write" }
        },
        
        // Web application client (SPA)
        new Client
        {
            ClientId = "web-app",
            ClientName = "Web Application",
            AllowedGrantTypes = GrantTypes.Code,
            RequireClientSecret = false,
            RequirePkce = true,
            
            RedirectUris = { "http://localhost:3000/callback", "http://localhost:4200/callback" },
            PostLogoutRedirectUris = { "http://localhost:3000", "http://localhost:4200" },
            AllowedCorsOrigins = { "http://localhost:3000", "http://localhost:4200" },
            
            AllowedScopes = {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                IdentityServerConstants.StandardScopes.Email,
                "catalog.read",
                "catalog.write",
                "users.read",
                "users.write"
            },
            
            AccessTokenLifetime = 3600,
            RefreshTokenUsage = TokenUsage.ReUse
        },
        
        // Mobile application client
        new Client
        {
            ClientId = "mobile-app",
            ClientName = "Mobile Application",
            AllowedGrantTypes = GrantTypes.Code,
            RequireClientSecret = false,
            RequirePkce = true,
            
            RedirectUris = { "com.company.app://callback" },
            
            AllowedScopes = {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                IdentityServerConstants.StandardScopes.Email,
                "catalog.read",
                "users.read"
            },
            
            AccessTokenLifetime = 3600,
            RefreshTokenUsage = TokenUsage.ReUse
        },
        
        // Postman/Testing client
        new Client
        {
            ClientId = "postman-client",
            ClientName = "Postman Testing Client",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("postman-secret".Sha256()) },
            AllowedScopes = { 
                "catalog.read", 
                "catalog.write", 
                "users.read", 
                "users.write",
                "admin"
            }
        }
    };
}
