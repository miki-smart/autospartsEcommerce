using Catalog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.Domain.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IReadOnlyList<Product>> GetProductsByCategory(Guid categoryId);
        Task<IReadOnlyList<Product>> SearchProducts(string searchTerm);
    }
}
