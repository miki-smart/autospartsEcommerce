using FluentValidation;

namespace Catalog.Application.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
    {
        public DeleteProductCommandValidator()
        {
            RuleFor(v => v.Id)
                .NotEmpty().WithMessage("Product Id is required.");
        }
    }
}
