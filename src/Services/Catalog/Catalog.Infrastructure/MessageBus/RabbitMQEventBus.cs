using System.Text;
using System.Text.Json;
using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Catalog.Infrastructure.MessageBus
{
    public class RabbitMQEventBus : IEventBus, IDisposable
    {
        private readonly RabbitMQSettings _settings;
        private readonly ILogger<RabbitMQEventBus> _logger;
        private IConnection _connection;
        private IModel _channel;
        private bool _disposed;

        public RabbitMQEventBus(IOptions<RabbitMQSettings> settings, ILogger<RabbitMQEventBus> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _settings.HostName,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    VirtualHost = _settings.VirtualHost,
                    Port = _settings.Port
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(
                    exchange: _settings.ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                _channel.ConfirmSelect();

                _connection.ConnectionShutdown += OnConnectionShutdown;

                _logger.LogInformation("RabbitMQ connection established");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not initialize RabbitMQ connection");
                throw;
            }
        }

        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RabbitMQEventBus));

            var eventName = @event.GetType().Name;
            var routingKey = $"catalog.events.{eventName.ToLower()}";
            
            try
            {
                _logger.LogInformation("Publishing event {EventName} to RabbitMQ", eventName);
                
                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = _channel.CreateBasicProperties();
                properties.DeliveryMode = 2; // persistent
                properties.ContentType = "application/json";
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                _channel.BasicPublish(
                    exchange: _settings.ExchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: body
                );

                _channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));

                _logger.LogInformation("Event {EventName} published successfully", eventName);
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing event {EventName}", eventName);
                throw;
            }
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogWarning("RabbitMQ connection shutdown. Reason: {0}", e.ReplyText);
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
