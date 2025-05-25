namespace Catalog.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string SKU { get; private set; }
        public decimal Price { get; private set; }
        public int StockQuantity { get; private set; }
        public string ImageUrl { get; private set; }
        public string Manufacturer { get; private set; }
        public string Model { get; private set; }
        public int Year { get; private set; }
        public Guid CategoryId { get; private set; }
        public Category Category { get; private set; }

        // For EF Core
        protected Product() { }

        public Product(
            string name, 
            string description, 
            string sku, 
            decimal price, 
            int stockQuantity, 
            string imageUrl, 
            string manufacturer, 
            string model, 
            int year, 
            Guid categoryId)
        {
            Name = name;
            Description = description;
            SKU = sku;
            Price = price;
            StockQuantity = stockQuantity;
            ImageUrl = imageUrl;
            Manufacturer = manufacturer;
            Model = model;
            Year = year;
            CategoryId = categoryId;
        }

        public void Update(
            string name, 
            string description, 
            decimal price, 
            int stockQuantity, 
            string imageUrl, 
            string manufacturer, 
            string model, 
            int year)
        {
            Name = name;
            Description = description;
            Price = price;
            StockQuantity = stockQuantity;
            ImageUrl = imageUrl;
            Manufacturer = manufacturer;
            Model = model;
            Year = year;
            ModifiedAt = DateTime.UtcNow;
        }

        public void UpdateStock(int quantity)
        {
            StockQuantity = quantity;
            ModifiedAt = DateTime.UtcNow;
        }

        public void ChangeCategory(Guid categoryId)
        {
            CategoryId = categoryId;
            ModifiedAt = DateTime.UtcNow;
        }
    }
}
