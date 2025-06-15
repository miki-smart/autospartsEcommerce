using Catalog.Application;
using Catalog.Infrastructure;
using Catalog.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add health checks
builder.Services.AddHealthChecks();

// Register application layers
try 
{
    builder.Services.AddApplication();
    builder.Services.AddPersistence(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);
}
catch
{
    // Ignore registration errors for now
}

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

// Register with Consul on startup
_ = Task.Run(async () =>
{
    try
    {
        await Task.Delay(5000); // Wait for service to be ready
        
        var httpClient = new HttpClient();
        var consulUrl = Environment.GetEnvironmentVariable("CONSUL_URL") ?? "http://consul:8500";
        var serviceId = Environment.GetEnvironmentVariable("SERVICE_ID") ?? "catalog-api";
        var serviceName = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "catalog-api";
        var servicePort = Environment.GetEnvironmentVariable("SERVICE_PORT") ?? "80";
        var serviceAddress = Environment.GetEnvironmentVariable("SERVICE_ADDRESS") ?? "catalog-api";
        
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
