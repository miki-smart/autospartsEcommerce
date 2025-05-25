using Catalog.Domain.Entities;
using MediatR;

namespace Catalog.Application.Products.Queries.SearchProducts
{
    public class SearchProductsQuery : IRequest<IReadOnlyList<Product>>
    {
        public string SearchTerm { get; }

        public SearchProductsQuery(string searchTerm)
        {
            SearchTerm = searchTerm;
        }
    }
}
