namespace Catalog.Domain.Events
{
    public class ProductDeletedEvent : DomainEvent
    {
        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; }
        public string ProductSKU { get; private set; }

        public ProductDeletedEvent(Guid productId, string productName, string productSku)
        {
            ProductId = productId;
            ProductName = productName;
            ProductSKU = productSku;
        }
    }
}
