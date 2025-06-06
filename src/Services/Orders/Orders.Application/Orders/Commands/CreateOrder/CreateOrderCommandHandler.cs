using MediatR;
using Orders.Application.Common.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Events;
using Orders.Domain.Repositories;

namespace Orders.Application.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEventBus _eventBus;

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            IEventBus eventBus)
        {
            _orderRepository = orderRepository;
            _eventBus = eventBus;
        }

        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var order = new Order(
                request.CustomerId,
                request.ShippingAddress,
                request.BillingAddress);

            foreach (var item in request.OrderItems)
            {
                order.AddOrderItem(
                    item.ProductId,
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice);
            }

            await _orderRepository.AddAsync(order);

            // Publish domain event
            var orderCreatedEvent = new OrderCreatedEvent(
                order.Id,
                order.OrderNumber,
                order.CustomerId,
                order.TotalAmount,
                order.OrderDate);

            await _eventBus.PublishAsync(orderCreatedEvent);

            return order.Id;
        }
    }
}
