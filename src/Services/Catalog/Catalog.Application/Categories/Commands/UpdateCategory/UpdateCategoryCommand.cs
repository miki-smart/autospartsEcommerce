using MediatR;
using Catalog.Domain.Entities;

namespace Catalog.Application.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryCommand : IRequest<Category>
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
    }
}
