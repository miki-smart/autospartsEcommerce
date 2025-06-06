using Orders.Application.Common.Interfaces;
using Orders.Infrastructure.MessageBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Orders.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure RabbitMQ settings
            services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQ"));

            // Register Event Bus - conditional based on configuration
            var useRabbitMQ = configuration.GetValue<bool>("UseRabbitMQ", false);
            
            if (useRabbitMQ)
            {
                services.AddScoped<IEventBus, RabbitMQEventBus>();
            }
            else
            {
                services.AddScoped<IEventBus, NoOpEventBus>();
            }

            // Add Redis caching (optional)
            var redisConnectionString = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                });
            }

            return services;
        }
    }
}
