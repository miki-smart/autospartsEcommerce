using Catalog.Domain.Entities;
using MediatR;

namespace Catalog.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommand : IRequest<Product>
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string SKU { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public required string ImageUrl { get; set; }
        public required string Manufacturer { get; set; }
        public required string Model { get; set; }
        public int Year { get; set; }
        public Guid CategoryId { get; set; }
    }
}
