using FluentValidation;

namespace Orders.Application.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty()
                .WithMessage("Customer ID is required");

            RuleFor(x => x.ShippingAddress)
                .NotEmpty()
                .WithMessage("Shipping address is required")
                .MaximumLength(500)
                .WithMessage("Shipping address cannot exceed 500 characters");

            RuleFor(x => x.BillingAddress)
                .NotEmpty()
                .WithMessage("Billing address is required")
                .MaximumLength(500)
                .WithMessage("Billing address cannot exceed 500 characters");

            RuleFor(x => x.OrderItems)
                .NotEmpty()
                .WithMessage("At least one order item is required");

            RuleForEach(x => x.OrderItems)
                .SetValidator(new OrderItemDtoValidator());
        }
    }

    public class OrderItemDtoValidator : AbstractValidator<OrderItemDto>
    {
        public OrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required");

            RuleFor(x => x.ProductName)
                .NotEmpty()
                .WithMessage("Product name is required")
                .MaximumLength(200)
                .WithMessage("Product name cannot exceed 200 characters");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Unit price cannot be negative");
        }
    }
}
