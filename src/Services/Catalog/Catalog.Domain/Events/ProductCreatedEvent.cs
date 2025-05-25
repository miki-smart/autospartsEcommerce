using Catalog.Domain.Entities;

namespace Catalog.Domain.Events
{
    public class ProductCreatedEvent : DomainEvent
    {
        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; }
        public string ProductSKU { get; private set; }

        public ProductCreatedEvent(Product product)
        {
            ProductId = product.Id;
            ProductName = product.Name;
            ProductSKU = product.SKU;
        }
    }
}
