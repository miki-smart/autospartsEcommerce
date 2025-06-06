using Identity.Application;
using Identity.Infrastructure;
using Identity.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for comprehensive logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/identity-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddApplication();
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

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// CORS must come before IdentityServer
app.UseCors("AllowWebApp");

// IdentityServer middleware - handles OAuth2/OIDC endpoints
app.UseIdentityServer();

// Authentication middleware - validates tokens
app.UseAuthentication();
app.UseAuthorization();

// MVC endpoints for login/logout UI
app.MapDefaultControllerRoute();

// Health check endpoint
app.MapHealthChecks("/health");

// Root endpoint with information
app.MapGet("/", () => "AutoParts Identity API - Navigate to /health for status")
    .WithOpenApi();

Log.Information("Starting AutoParts Identity API...");

app.Run();
