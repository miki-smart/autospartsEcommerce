using Catalog.Domain.Entities;
using MediatR;

namespace Catalog.Application.Products.Queries.GetAllProducts
{
    public class GetAllProductsQuery : IRequest<IReadOnlyList<Product>>
    {
    }
}
