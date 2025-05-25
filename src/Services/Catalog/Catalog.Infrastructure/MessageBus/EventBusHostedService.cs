using Catalog.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Catalog.Infrastructure.MessageBus
{
    public class EventBusHostedService : IHostedService
    {
        private readonly RabbitMQConsumer _consumer;
        private readonly ILogger<EventBusHostedService> _logger;

        public EventBusHostedService(
            RabbitMQConsumer consumer,
            ILogger<EventBusHostedService> logger)
        {
            _consumer = consumer;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting EventBusHostedService");
            
            // Register event handlers
            // Example: _consumer.Subscribe<ProductCreatedEvent, ProductCreatedEventHandler>();
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping EventBusHostedService");
            _consumer.Dispose();
            
            return Task.CompletedTask;
        }
    }
}
