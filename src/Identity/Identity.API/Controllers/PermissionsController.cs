using Identity.Application.Features.Management.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PermissionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Permission CRUD
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePermissionCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(await _mediator.Send(command));
        }

        // Add endpoints for categorizing permissions by service, etc.
    }
}
