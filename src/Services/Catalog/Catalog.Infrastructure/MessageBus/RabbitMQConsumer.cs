using System;
using System.Text;
using System.Text.Json;
using Catalog.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace Catalog.Infrastructure.MessageBus
{
    public class RabbitMQConsumer : IDisposable
    {
        private readonly RabbitMQSettings _settings;
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;
        private bool _disposed;
        private string _queueName;

        public RabbitMQConsumer(
            RabbitMQSettings settings,
            ILogger<RabbitMQConsumer> logger,
            IServiceProvider serviceProvider)
        {
            _settings = settings;
            _logger = logger;
            _serviceProvider = serviceProvider;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            try
            {                var factory = new ConnectionFactory
                {
                    HostName = _settings.HostName,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    VirtualHost = _settings.VirtualHost,
                    Port = _settings.Port
                    // DispatchConsumersAsync removed for compatibility with RabbitMQ.Client 6.5.0
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(
                    exchange: _settings.ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                _queueName = _channel.QueueDeclare(
                    queue: $"catalog_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false).QueueName;

                _logger.LogInformation("RabbitMQ consumer initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not initialize RabbitMQ consumer");
                throw;
            }
        }

        public void Subscribe<TEvent, TEventHandler>()
            where TEvent : DomainEvent
            where TEventHandler : IDomainEventHandler<TEvent>
        {
            var eventName = typeof(TEvent).Name;
            var routingKey = $"catalog.events.{eventName.ToLower()}";

            _channel.QueueBind(
                queue: _queueName,
                exchange: _settings.ExchangeName,
                routingKey: routingKey);

            _logger.LogInformation("Subscribing to event {EventName} with routing key {RoutingKey}", eventName, routingKey);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (_, eventArgs) =>
            {
                var eventMessage = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                var @event = JsonSerializer.Deserialize<TEvent>(eventMessage);

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<TEventHandler>();
                    
                    if (handler != null)
                    {
                        await handler.Handle(@event);
                        _channel.BasicAck(eventArgs.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogWarning("No handler registered for {EventName}", eventName);
                        _channel.BasicNack(eventArgs.DeliveryTag, false, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event {EventName}", eventName);
                    _channel.BasicNack(eventArgs.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _channel?.Close();
                _channel?.Dispose();
                _channel = null;

                _connection?.Close();
                _connection?.Dispose();
                _connection = null;
            }

            _disposed = true;
        }
    }
}
