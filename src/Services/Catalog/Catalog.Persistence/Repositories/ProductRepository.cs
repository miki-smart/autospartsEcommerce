using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Persistence.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(CatalogDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Product>> GetProductsByCategory(Guid categoryId)
        {
            return await _dbContext.Products
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Product>> SearchProducts(string searchTerm)
        {
            return await _dbContext.Products
                .Where(p => p.Name.Contains(searchTerm) ||
                            p.Description.Contains(searchTerm) ||
                            p.Manufacturer.Contains(searchTerm) ||
                            p.Model.Contains(searchTerm))
                .ToListAsync();
        }

        // Override to include the Category navigation property
        public new async Task<Product> GetByIdAsync(Guid id)
        {
            return await _dbContext.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public new async Task<IReadOnlyList<Product>> GetAllAsync()
        {
            return await _dbContext.Products
                .Include(p => p.Category)
                .ToListAsync();
        }
    }
}
