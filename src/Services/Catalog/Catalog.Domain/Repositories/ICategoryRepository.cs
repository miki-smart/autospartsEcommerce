using Catalog.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Catalog.Domain.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category> GetCategoryWithProductsAsync(Guid id);
    }
}
