using Orders.Domain.Entities;

namespace Orders.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; private set; } = string.Empty;
        public Guid CustomerId { get; private set; }
        public OrderStatus Status { get; private set; }
        public decimal TotalAmount { get; private set; }
        public DateTime OrderDate { get; private set; }
        public string ShippingAddress { get; private set; } = string.Empty;
        public string BillingAddress { get; private set; } = string.Empty;

        private readonly List<OrderItem> _orderItems = new();
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

        protected Order() { } // For EF Core

        public Order(Guid customerId, string shippingAddress, string billingAddress)
        {
            CustomerId = customerId;
            OrderNumber = GenerateOrderNumber();
            Status = OrderStatus.Pending;
            OrderDate = DateTime.UtcNow;
            ShippingAddress = shippingAddress;
            BillingAddress = billingAddress;
            TotalAmount = 0;
        }

        public void AddOrderItem(Guid productId, string productName, int quantity, decimal unitPrice)
        {
            var orderItem = new OrderItem(Id, productId, productName, quantity, unitPrice);
            _orderItems.Add(orderItem);
            RecalculateTotalAmount();
        }

        public void UpdateStatus(OrderStatus newStatus)
        {
            Status = newStatus;
            ModifiedAt = DateTime.UtcNow;
        }

        private void RecalculateTotalAmount()
        {
            TotalAmount = _orderItems.Sum(item => item.TotalPrice);
        }

        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
    }

    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Processing = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5,
        Refunded = 6
    }
}
