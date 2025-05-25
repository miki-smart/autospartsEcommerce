using Catalog.Application.Products.Commands.CreateProduct;
using Catalog.Application.Products.Commands.DeleteProduct;
using Catalog.Application.Products.Commands.UpdateProduct;
using Catalog.Application.Products.Queries.GetAllProducts;
using Catalog.Application.Products.Queries.GetProductById;
using Catalog.Application.Products.Queries.GetProductsByCategory;
using Catalog.Application.Products.Queries.SearchProducts;
using Catalog.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts()
        {
            var products = await _mediator.Send(new GetAllProductsQuery());
            return Ok(products);
        }        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductById(Guid id)
        {
            try
            {
                var product = await _mediator.Send(new GetProductByIdQuery(id));
                return Ok(product);
            }
            catch (Catalog.Application.Common.Exceptions.NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IReadOnlyList<Product>>> GetProductsByCategory(Guid categoryId)
        {
            var products = await _mediator.Send(new GetProductsByCategoryQuery(categoryId));
            return Ok(products);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyList<Product>>> SearchProducts([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term cannot be empty");
                
            var products = await _mediator.Send(new SearchProductsQuery(searchTerm));
            return Ok(products);
        }        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] CreateProductCommand command)
        {
            try
            {
                var product = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (Catalog.Application.Common.Exceptions.ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
        }[HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<Product>> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command)
        {
            // Ensure the ID in the route matches the ID in the command
            if (id != command.Id)
                return BadRequest("ID mismatch");
                
            try
            {
                var product = await _mediator.Send(command);
                return Ok(product);
            }
            catch (Catalog.Application.Common.Exceptions.NotFoundException)
            {
                return NotFound();
            }
            catch (Catalog.Application.Common.Exceptions.ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteProduct(Guid id)
        {
            var result = await _mediator.Send(new DeleteProductCommand { Id = id });
            
            if (!result)
                return NotFound();
                
            return NoContent();
        }
    }
}
