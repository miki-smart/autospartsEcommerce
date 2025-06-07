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

        [HttpPut("profile-picture")]
        public async Task<IActionResult> UpdateProfilePicture([FromBody] string profilePictureUrl)
        {
            var command = new UpdateProfileCommand { ProfilePicture = profilePictureUrl };
            return Ok(await _mediator.Send(command));
        }

        /// <summary>
        /// Gets the login history for the current user
        /// </summary>
        [HttpGet("login-history")]
        public async Task<IActionResult> GetLoginHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var query = new Identity.Application.Features.Users.Queries.GetLoginHistoryQuery
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Admin: Get all users with search, filter, and pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] string? role = null, [FromQuery] bool? isActive = null)
        {
            var query = new Identity.Application.Features.Users.Queries.GetUsersQuery
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = search,
                Role = role,
                IsActive = isActive
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Admin: Get user details by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var query = new Identity.Application.Features.Users.Queries.GetUserByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            if (!result.Success || result.Data == null)
                return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Admin: Activate a user account
        /// </summary>
        [HttpPost("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateUser(string id)
        {
            var command = new Identity.Application.Features.Users.Commands.ActivateUserCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Admin: Deactivate a user account
        /// </summary>
        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            var command = new Identity.Application.Features.Users.Commands.DeactivateUserCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Admin: Lock a user account
        /// </summary>
        [HttpPost("{id}/lock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LockUser(string id)
        {
            // Locking can be implemented as deactivation or by setting lockout properties
            var command = new Identity.Application.Features.Users.Commands.DeactivateUserCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Admin: Unlock a user account
        /// </summary>
        [HttpPost("{id}/unlock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnlockUser(string id)
        {
            // Unlocking can be implemented as activation or by resetting lockout properties
            var command = new Identity.Application.Features.Users.Commands.ActivateUserCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
