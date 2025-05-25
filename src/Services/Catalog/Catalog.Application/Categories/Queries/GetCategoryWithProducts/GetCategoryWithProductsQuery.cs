using Catalog.Domain.Entities;
using MediatR;

namespace Catalog.Application.Categories.Queries.GetCategoryWithProducts
{
    public class GetCategoryWithProductsQuery : IRequest<Category>
    {
        public Guid Id { get; }

        public GetCategoryWithProductsQuery(Guid id)
        {
            Id = id;
        }
    }
}
