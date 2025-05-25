using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using MediatR;
using Catalog.Application.Common.Exceptions;

namespace Catalog.Application.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Product>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProductByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Product> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
            
            if (product == null)
                throw new NotFoundException(nameof(Product), request.Id);
                
            return product;
        }
    }
}
