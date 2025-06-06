using Orders.Domain.Entities;

namespace Orders.Domain.Entities
{    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; } = string.Empty;
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal TotalPrice => Quantity * UnitPrice;

        protected OrderItem() { } // For EF Core

        public OrderItem(Guid orderId, Guid productId, string productName, int quantity, decimal unitPrice)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
            if (unitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Product name is required", nameof(productName));

            OrderId = orderId;
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));
            
            Quantity = newQuantity;
            ModifiedAt = DateTime.UtcNow;
        }
    }
}
