using FluentValidation;

namespace Catalog.Application.Products.Queries.GetProductsByCategory
{
    public class GetProductsByCategoryQueryValidator : AbstractValidator<GetProductsByCategoryQuery>
    {
        public GetProductsByCategoryQueryValidator()
        {
            RuleFor(v => v.CategoryId)
                .NotEmpty().WithMessage("Category Id is required.");
        }
    }
}
