using Orders.Application;
using Orders.Infrastructure;
using Orders.Persistence;
using Orders.Persistence.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

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
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created (for development)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

// Register with Consul on startup
_ = Task.Run(async () =>
{
    try
    {
        await Task.Delay(5000); // Wait for service to be ready
        
        var httpClient = new HttpClient();
        var consulUrl = Environment.GetEnvironmentVariable("CONSUL_URL") ?? "http://consul:8500";
        var serviceId = Environment.GetEnvironmentVariable("SERVICE_ID") ?? "orders-api";
        var serviceName = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "orders-api";
        var servicePort = Environment.GetEnvironmentVariable("SERVICE_PORT") ?? "80";
        var serviceAddress = Environment.GetEnvironmentVariable("SERVICE_ADDRESS") ?? "orders-api";
        
        var registration = new
        {
            ID = serviceId,
            Name = serviceName,
            Address = serviceAddress,
            Port = int.Parse(servicePort),
            Check = new
            {
                HTTP = $"http://{serviceAddress}:{servicePort}/health",
                Interval = "30s",
                DeregisterCriticalServiceAfter = "90s"
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(registration);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var response = await httpClient.PutAsync($"{consulUrl}/v1/agent/service/register", content);
        Console.WriteLine($"Consul registration response: {response.StatusCode}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to register with Consul: {ex.Message}");
    }
});

app.Run();
