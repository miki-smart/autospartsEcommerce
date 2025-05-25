using FluentValidation;

namespace Catalog.Application.Products.Queries.SearchProducts
{
    public class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
    {
        public SearchProductsQueryValidator()
        {
            RuleFor(v => v.SearchTerm)
                .NotEmpty().WithMessage("Search term is required.")
                .MinimumLength(2).WithMessage("Search term must be at least 2 characters.");
        }
    }
}
