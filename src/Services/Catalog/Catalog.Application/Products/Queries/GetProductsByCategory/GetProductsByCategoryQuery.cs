using Catalog.Domain.Entities;
using MediatR;

namespace Catalog.Application.Products.Queries.GetProductsByCategory
{
    public class GetProductsByCategoryQuery : IRequest<IReadOnlyList<Product>>
    {
        public Guid CategoryId { get; }

        public GetProductsByCategoryQuery(Guid categoryId)
        {
            CategoryId = categoryId;
        }
    }
}
