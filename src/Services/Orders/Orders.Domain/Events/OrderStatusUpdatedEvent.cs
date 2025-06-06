using Orders.Domain.Events;
using Orders.Domain.Entities;

namespace Orders.Domain.Events
{
    public class OrderStatusUpdatedEvent : DomainEvent
    {
        public Guid OrderId { get; }
        public string OrderNumber { get; }
        public OrderStatus PreviousStatus { get; }
        public OrderStatus NewStatus { get; }

        public OrderStatusUpdatedEvent(Guid orderId, string orderNumber, OrderStatus previousStatus, OrderStatus newStatus)
        {
            OrderId = orderId;
            OrderNumber = orderNumber;
            PreviousStatus = previousStatus;
            NewStatus = newStatus;
        }
    }
}
