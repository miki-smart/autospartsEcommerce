using Identity.Application.Features.Management.Commands;
using Identity.Application.Features.Management.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // User profile endpoints
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile() => Ok(await _mediator.Send(new GetProfileQuery()));

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(await _mediator.Send(command));
        }

        // User-roles and user-permissions endpoints can be added here
    }
}
