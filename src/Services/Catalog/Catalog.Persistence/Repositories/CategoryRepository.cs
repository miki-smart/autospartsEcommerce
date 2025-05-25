using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Persistence.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(CatalogDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Category> GetCategoryWithProductsAsync(Guid id)
        {
            return await _dbContext.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
