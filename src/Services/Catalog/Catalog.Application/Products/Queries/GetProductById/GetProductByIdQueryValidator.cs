using FluentValidation;

namespace Catalog.Application.Products.Queries.GetProductById
{
    public class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
    {
        public GetProductByIdQueryValidator()
        {
            RuleFor(v => v.Id)
                .NotEmpty().WithMessage("Product Id is required.");
        }
    }
}
