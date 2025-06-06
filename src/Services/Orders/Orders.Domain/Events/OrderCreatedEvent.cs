using Orders.Domain.Events;

namespace Orders.Domain.Events
{
    public class OrderCreatedEvent : DomainEvent
    {
        public Guid OrderId { get; }
        public string OrderNumber { get; }
        public Guid CustomerId { get; }
        public decimal TotalAmount { get; }
        public DateTime OrderDate { get; }

        public OrderCreatedEvent(Guid orderId, string orderNumber, Guid customerId, decimal totalAmount, DateTime orderDate)
        {
            OrderId = orderId;
            OrderNumber = orderNumber;
            CustomerId = customerId;
            TotalAmount = totalAmount;
            OrderDate = orderDate;
        }
    }
}
