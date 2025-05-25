using Catalog.Application.Categories.Commands.CreateCategory;
using Catalog.Application.Categories.Commands.DeleteCategory;
using Catalog.Application.Categories.Commands.UpdateCategory;
using Catalog.Application.Categories.Queries.GetAllCategories;
using Catalog.Application.Categories.Queries.GetCategoryById;
using Catalog.Application.Categories.Queries.GetCategoryWithProducts;
using Catalog.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Category>>> GetCategories()
        {
            var categories = await _mediator.Send(new GetAllCategoriesQuery());
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategoryById(Guid id)
        {
            var category = await _mediator.Send(new GetCategoryByIdQuery(id));
            
            if (category == null)
                return NotFound();
                
            return Ok(category);
        }

        [HttpGet("{id}/products")]
        public async Task<ActionResult<Category>> GetCategoryWithProducts(Guid id)
        {
            var category = await _mediator.Send(new GetCategoryWithProductsQuery(id));
            
            if (category == null)
                return NotFound();
                
            return Ok(category);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] CreateCategoryCommand command)
        {
            var category = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<Category>> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command)
        {
            // Ensure the ID in the route matches the ID in the command
            if (id != command.Id)
                return BadRequest("ID mismatch");
                
            try
            {
                var category = await _mediator.Send(command);
                return Ok(category);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteCategory(Guid id)
        {
            var result = await _mediator.Send(new DeleteCategoryCommand { Id = id });
            
            if (!result)
                return NotFound();
                
            return NoContent();
        }
    }
}
