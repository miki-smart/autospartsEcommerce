using Identity.Application.Features.Management.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Role CRUD
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(await _mediator.Send(command));
        }

        // Assign role to user
        [HttpPost("assign-user")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleToUserCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(await _mediator.Send(command));
        }

        // Assign permission to role
        [HttpPost("assign-permission")]
        public async Task<IActionResult> AssignPermissionToRole([FromBody] AssignPermissionToRoleCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(await _mediator.Send(command));
        }

        // Add more CRUD endpoints as needed
    }
}
