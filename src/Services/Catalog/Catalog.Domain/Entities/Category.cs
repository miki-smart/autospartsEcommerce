namespace Catalog.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public ICollection<Product> Products { get; private set; }

        // For EF Core
        protected Category() { }

        public Category(string name, string description)
        {
            Name = name;
            Description = description;
            Products = new List<Product>();
        }

        public void Update(string name, string description)
        {
            Name = name;
            Description = description;
            ModifiedAt = DateTime.UtcNow;
        }
    }
}
