using Catalog.Domain.Entities;
using MediatR;

namespace Catalog.Application.Categories.Queries.GetAllCategories
{
    public class GetAllCategoriesQuery : IRequest<IReadOnlyList<Category>>
    {
    }
}
