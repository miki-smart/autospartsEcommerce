using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Domain.Events;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Product>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly ILogger<UpdateProductCommandHandler> _logger;

        public UpdateProductCommandHandler(
            IUnitOfWork unitOfWork,
            IEventBus eventBus,
            ILogger<UpdateProductCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task<Product> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
                
                if (product == null)
                    throw new NotFoundException(nameof(Product), request.Id);

                // Store old values for event
                decimal oldPrice = product.Price;
                int oldStockQuantity = product.StockQuantity;

                // Update product
                product.Update(
                    request.Name,
                    request.Description,
                    request.Price,
                    request.StockQuantity,
                    request.ImageUrl,
                    request.Manufacturer,
                    request.Model,
                    request.Year);

                if (request.CategoryId.HasValue && request.CategoryId.Value != product.CategoryId)
                {
                    product.ChangeCategory(request.CategoryId.Value);
                }
                
                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();
                
                // Publish event
                var domainEvent = new ProductUpdatedEvent(product, oldPrice, oldStockQuantity);
                await _eventBus.PublishAsync(domainEvent);
                
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling UpdateProductCommand");
                throw;
            }
        }
    }
}
