using Catalog.Domain.Entities;
using MediatR;

namespace Catalog.Application.Products.Queries.GetProductById
{
    public class GetProductByIdQuery : IRequest<Product>
    {
        public Guid Id { get; set; }

        public GetProductByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
