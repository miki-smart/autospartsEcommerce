using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Category>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateCategoryCommandHandler> _logger;

        public CreateCategoryCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateCategoryCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Category> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var category = new Category(request.Name, request.Description);

                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();
                
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling CreateCategoryCommand");
                throw;
            }
        }
    }
}
