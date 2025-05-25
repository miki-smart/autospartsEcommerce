using Catalog.Domain.Entities;

namespace Catalog.Domain.Events
{
    public class ProductUpdatedEvent : DomainEvent
    {
        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; }
        public decimal OldPrice { get; private set; }
        public decimal NewPrice { get; private set; }
        public int OldStockQuantity { get; private set; }
        public int NewStockQuantity { get; private set; }

        public ProductUpdatedEvent(Product product, decimal oldPrice, int oldStockQuantity)
        {
            ProductId = product.Id;
            ProductName = product.Name;
            OldPrice = oldPrice;
            NewPrice = product.Price;
            OldStockQuantity = oldStockQuantity;
            NewStockQuantity = product.StockQuantity;
        }
    }
}
