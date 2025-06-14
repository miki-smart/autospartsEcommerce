using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using Ocelot.Provider.Consul;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AspNetCoreRateLimit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using HealthChecks.Consul;
using ApiGateway.Middleware;
using HealthChecks.Uris;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Linq; // Added for .Any()
using ApiGateway.Services; // Added using statement for ApiGateway.Services

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ocelot with Consul and Polly
builder.Services.AddOcelot()
    .AddConsul()
    .AddPolly();

// JWT Authentication
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettingsSection["SecretKey"] ?? throw new ArgumentNullException(nameof(jwtSettingsSection), "JWT SecretKey (JwtSettings:SecretKey) must be configured.");
var issuer = jwtSettingsSection["Issuer"] ?? throw new ArgumentNullException(nameof(jwtSettingsSection), "JWT Issuer (JwtSettings:Issuer) must be configured.");
var audience = jwtSettingsSection["Audience"] ?? throw new ArgumentNullException(nameof(jwtSettingsSection), "JWT Audience (JwtSettings:Audience) must be configured.");
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("JwtSettings:RequireHttpsMetadata", false);
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// CORS
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();
if (allowedOrigins == null || !allowedOrigins.Any())
{
    allowedOrigins = new[] { "http://localhost:3000" }; // Default for local dev
    Log.Warning("CORS AllowedOrigins not configured in appsettings.json. Using default: {DefaultOrigins}", string.Join(", ", allowedOrigins));
}
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});


// IP Rate Limiting
builder.Services.AddOptions();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Health Checks
var healthChecksBuilder = builder.Services.AddHealthChecks();

Action<HttpClientHandler> configureHttpClientHandler = handler =>
{
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
};

var identityApiUrlString = builder.Configuration["HealthChecksUris:IdentityAPI"];
if (string.IsNullOrWhiteSpace(identityApiUrlString)) throw new ArgumentNullException("HealthChecksUris:IdentityAPI must be configured.");
healthChecksBuilder.AddUrlGroup(new Uri(identityApiUrlString), name: "Identity Service", failureStatus: HealthStatus.Degraded, configurePrimaryHttpMessageHandler: _ => new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });

var productCatalogApiUrlString = builder.Configuration["HealthChecksUris:ProductCatalogAPI"];
if (string.IsNullOrWhiteSpace(productCatalogApiUrlString)) throw new ArgumentNullException("HealthChecksUris:ProductCatalogAPI must be configured.");
healthChecksBuilder.AddUrlGroup(new Uri(productCatalogApiUrlString), name: "Product Catalog Service", failureStatus: HealthStatus.Degraded, configurePrimaryHttpMessageHandler: _ => new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });

string[] serviceKeys = { "InventoryAPI", "OrderAPI", "PaymentAPI", "NotificationAPI", "LogisticsAPI", "SellerManagementAPI", "SearchAPI" };
foreach (var serviceKey in serviceKeys)
{
    var serviceUrlString = builder.Configuration[$"HealthChecksUris:{serviceKey}"];
    if (string.IsNullOrWhiteSpace(serviceUrlString)) throw new ArgumentNullException($"HealthChecksUris:{serviceKey} must be configured.");
    healthChecksBuilder.AddUrlGroup(new Uri(serviceUrlString), name: $"{serviceKey.Replace("API", " Service")}", failureStatus: HealthStatus.Degraded, configurePrimaryHttpMessageHandler: _ => new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });
}

// Consul Health Check - basic configuration
var consulHostUrlString = builder.Configuration["Consul:Host"];
if (!string.IsNullOrWhiteSpace(consulHostUrlString))
{
    healthChecksBuilder.AddConsul(setup => 
    {
        // Basic Consul health check - checks if Consul is reachable
        // The setup parameter is ConsulOptions from HealthChecks.Consul
        // For now, use default settings - the library should use localhost:8500 by default
    }, name: "Consul Health Check", failureStatus: HealthStatus.Unhealthy);
}

builder.Services.AddHealthChecksUI(setupSettings: setup =>
{
    setup.SetEvaluationTimeInSeconds(builder.Configuration.GetValue<int>("HealthChecksUISettings:EvaluationTimeInSeconds", 10));
    setup.SetMinimumSecondsBetweenFailureNotifications(builder.Configuration.GetValue<int>("HealthChecksUISettings:MinimumSecondsBetweenFailureNotifications", 60));
}).AddInMemoryStorage();

builder.Services.AddScoped<IServiceConfigurationService, ServiceConfigurationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}

if (builder.Configuration.GetValue<bool>("EnableHttpsRedirection", true))
{
    app.UseHttpsRedirection();
}

app.UseSerilogRequestLogging();

// Custom Correlation ID middleware - must be early in the pipeline
app.UseCorrelationId();

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseIpRateLimiting();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Map diagnostic controller endpoints before Ocelot
app.MapControllers();

var healthUiPath = builder.Configuration.GetValue<string>("HealthChecksUISettings:UIPath");
if (string.IsNullOrEmpty(healthUiPath)) { healthUiPath = "/health-ui"; }
app.MapHealthChecksUI(options => { options.UIPath = healthUiPath; });

// Configure Ocelot conditionally - only for API routes, not for diagnostics/health
app.MapWhen(
    context => {
        var path = context.Request.Path.Value?.ToLowerInvariant();
        return path != null &&
               !path.StartsWith("/health") &&
               !path.StartsWith("/api/diagnostics") &&
               !path.StartsWith("/swagger") &&
               !path.StartsWith("/health-ui") &&
               !path.StartsWith("/api/servicemanagement"); // Exclude service management endpoints
    },
    ocelotApp => {
        ocelotApp.UseOcelot().Wait();
    });

// For non-Ocelot paths, ensure they are handled by the mapped endpoints above

app.Run();
