using Orders.Application;
using Orders.Infrastructure;
using Orders.Persistence;
using Orders.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Consul;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add health checks
builder.Services.AddHealthChecks();

// Add application layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Development specific configuration
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created (for development)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        // Skip database initialization for now
    }
}
catch
{
    // Ignore database initialization errors
}

app.Lifetime.ApplicationStarted.Register(() =>
{
    var consulClient = app.Services.GetRequiredService<IConsulClient>();
    var consulUrl = Environment.GetEnvironmentVariable("CONSUL_URL") ?? "http://consul:8500";
    var serviceId = Environment.GetEnvironmentVariable("SERVICE_ID") ?? "catalog-api";
    var serviceName = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "catalog-api";
    var servicePort = Environment.GetEnvironmentVariable("SERVICE_PORT") ?? "80";
    var serviceAddress = Environment.GetEnvironmentVariable("SERVICE_ADDRESS") ?? "catalog-api";
    var registration = new AgentServiceRegistration()
    {
        ID = serviceId,
        Name = serviceName,
        Address = serviceAddress, // docker container name
        Port = int.Parse(servicePort), // container's internal port
        Check = new AgentServiceCheck
        {
            HTTP = $"http://{serviceAddress}/health",
            Interval = TimeSpan.FromSeconds(10)
        }
    };
    consulClient.Agent.ServiceRegister(registration).Wait();
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    var consulClient = app.Services.GetRequiredService<IConsulClient>();
    var serviceId = Environment.GetEnvironmentVariable("SERVICE_ID") ?? "catalog-api";
    consulClient.Agent.ServiceDeregister(serviceId).Wait();
});
app.Run();
