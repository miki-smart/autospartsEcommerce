using FluentValidation;

namespace Catalog.Application.Categories.Queries.GetCategoryWithProducts
{
    public class GetCategoryWithProductsQueryValidator : AbstractValidator<GetCategoryWithProductsQuery>
    {
        public GetCategoryWithProductsQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Category Id is required.");
        }
    }
}
