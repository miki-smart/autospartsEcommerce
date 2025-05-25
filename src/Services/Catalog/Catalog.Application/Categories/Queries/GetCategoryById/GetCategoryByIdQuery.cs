using Catalog.Domain.Entities;
using MediatR;

namespace Catalog.Application.Categories.Queries.GetCategoryById
{
    public class GetCategoryByIdQuery : IRequest<Category>
    {
        public Guid Id { get; }

        public GetCategoryByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
