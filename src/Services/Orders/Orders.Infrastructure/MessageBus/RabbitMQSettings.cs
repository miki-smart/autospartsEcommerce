namespace Orders.Infrastructure.MessageBus
{
    public class RabbitMQSettings
    {
        public string HostName { get; set; } = "localhost";
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public int Port { get; set; } = 5672;
        public string ExchangeName { get; set; } = "orders_exchange";
        public string QueueName { get; set; } = "orders_queue";
    }
}
