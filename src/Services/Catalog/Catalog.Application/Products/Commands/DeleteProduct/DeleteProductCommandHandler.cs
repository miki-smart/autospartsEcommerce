using MediatR;
using Catalog.Domain.Repositories;
using Catalog.Domain.Events;
using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly ILogger<DeleteProductCommandHandler> _logger;

        public DeleteProductCommandHandler(
            IUnitOfWork unitOfWork,
            IEventBus eventBus,
            ILogger<DeleteProductCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling DeleteProductCommand");
                throw;
            }
        }
    }
}
