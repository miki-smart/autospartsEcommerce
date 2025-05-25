using MediatR;
using Catalog.Domain.Repositories;
using Catalog.Domain.Events;
using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Entities;

namespace Catalog.Application.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;

        public DeleteProductCommandHandler(
            IUnitOfWork unitOfWork,
            IEventBus eventBus)
        {
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
            
            if (product == null)
                return false;
                
            await _unitOfWork.Products.DeleteAsync(product);
            await _unitOfWork.SaveChangesAsync();
            
            // Publish event
            await _eventBus.PublishAsync(new ProductDeletedEvent(product.Id, product.Name, product.SKU));
            
            return true;
        }
    }
}
