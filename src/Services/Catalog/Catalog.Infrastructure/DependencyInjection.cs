using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Events;
using Catalog.Infrastructure.Cache;
using Catalog.Infrastructure.MessageBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Redis caching
            services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetValue<string>("CacheSettings:ConnectionString");
                options.InstanceName = configuration.GetValue<string>("CacheSettings:InstanceName");
            });
            services.AddScoped<ICacheService, RedisCacheService>();

            // Configure RabbitMQ messaging
            services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQSettings"));
            services.AddSingleton<RabbitMQConsumer>(sp =>
            {
                var settings = configuration.GetSection("RabbitMQSettings").Get<RabbitMQSettings>();
                var logger = sp.GetRequiredService<ILogger<RabbitMQConsumer>>();
                return new RabbitMQConsumer(settings, logger, sp);
            });
            services.AddScoped<IEventBus, RabbitMQEventBus>();
            services.AddHostedService<EventBusHostedService>();

            return services;
        }
    }
}
