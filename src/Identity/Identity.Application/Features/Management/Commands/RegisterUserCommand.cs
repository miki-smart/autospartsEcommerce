using MediatR;
using Identity.Application.Common.Models;

namespace Identity.Application.Features.Management.Commands
{
    public class RegisterUserCommand : IRequest<ApiResponse<string>>
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        // Add other registration fields as needed
    }
}
