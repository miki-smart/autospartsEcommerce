using Catalog.Domain.Entities;
using MediatR;

namespace Catalog.Application.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommand : IRequest<Category>
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
    }
}
