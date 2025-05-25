using Catalog.Application.Common.Exceptions;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Categories.Queries.GetCategoryWithProducts
{
    public class GetCategoryWithProductsQueryHandler : IRequestHandler<GetCategoryWithProductsQuery, Category>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCategoryWithProductsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Category> Handle(GetCategoryWithProductsQuery request, CancellationToken cancellationToken)
        {
            var category = await _unitOfWork.Categories.GetCategoryWithProductsAsync(request.Id);
            
            if (category == null)
                throw new NotFoundException(nameof(Category), request.Id);
                
            return category;
        }
    }
}
