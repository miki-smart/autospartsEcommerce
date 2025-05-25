using FluentValidation;

namespace Catalog.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
                
            RuleFor(v => v.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
                
            RuleFor(v => v.SKU)
                .NotEmpty().WithMessage("SKU is required.")
                .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters.");
                
            RuleFor(v => v.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");
                
            RuleFor(v => v.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");
                
            RuleFor(v => v.CategoryId)
                .NotEmpty().WithMessage("Category ID is required.");
        }
    }
}
