using Identity.Application;
using Identity.Infrastructure;
using Identity.Persistence;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for comprehensive logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/identity-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// Configure CORS for client applications
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add MVC for login/logout UI
builder.Services.AddControllersWithViews();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<Identity.Persistence.Context.ApplicationDbContext>();

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "AutoParts Identity API", 
        Version = "v1",
        Description = "Identity Server API for AutoParts Ecommerce platform providing authentication, authorization, and user management",
        Contact = new OpenApiContact
        {
            Name = "AutoParts Development Team",
            Email = "dev@autoparts.com"
        }
    });

    // Add JWT Bearer Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Include XML comments if available
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add rate limiting configuration
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // Enable Swagger middleware in development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// CORS must come before IdentityServer
app.UseCors("AllowWebApp");

// Enable rate limiting middleware
app.UseIpRateLimiting();

// IdentityServer middleware - handles OAuth2/OIDC endpoints
app.UseIdentityServer();

// Authentication middleware - validates tokens
app.UseAuthentication();
app.UseAuthorization();

// MVC endpoints for login/logout UI
app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// Root endpoint with information
app.MapGet("/", () => "AutoParts Identity API - Navigate to /health for status")
    .WithOpenApi();

Log.Information("Starting AutoParts Identity API...");

app.Run();
