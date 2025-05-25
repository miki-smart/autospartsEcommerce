using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using MediatR;
using Catalog.Application.Common.Exceptions;
using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Product>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly ILogger<CreateProductCommandHandler> _logger;

        public CreateProductCommandHandler(
            IUnitOfWork unitOfWork,
            IEventBus eventBus,
            ILogger<CreateProductCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task<Product> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var product = new Product(
                    request.Name,
                    request.Description,
                    request.SKU,
                    request.Price,
                    request.StockQuantity,
                    request.ImageUrl,
                    request.Manufacturer,
                    request.Model,
                    request.Year,
                    request.CategoryId);

                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();
                
                // Publish domain event
                var domainEvent = new ProductCreatedEvent(product);
                await _eventBus.PublishAsync(domainEvent);
                
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling CreateProductCommand");
                throw;
            }
        }
    }
}
