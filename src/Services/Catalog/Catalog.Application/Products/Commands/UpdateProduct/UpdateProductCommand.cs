using Catalog.Domain.Entities;
using MediatR;

namespace Catalog.Application.Products.Commands.UpdateProduct
{
    public class UpdateProductCommand : IRequest<Product>
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public required string ImageUrl { get; set; }
        public required string Manufacturer { get; set; }
        public required string Model { get; set; }
        public int Year { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
