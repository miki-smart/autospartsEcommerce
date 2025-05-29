using Catalog.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure.MessageBus
{
    /// <summary>
    /// No-operation event bus implementation for local development
    /// when RabbitMQ is not available or not needed
    /// </summary>
    public class NoOpEventBus : IEventBus
    {
        private readonly ILogger<NoOpEventBus> _logger;

        public NoOpEventBus(ILogger<NoOpEventBus> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
        {
            var eventName = @event.GetType().Name;
            _logger.LogInformation("NoOpEventBus: Would publish event {EventName} (no-op mode)", eventName);
            return Task.CompletedTask;
        }
    }
}
