using Identity.Application.Common.Models;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Application.Features.Management.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ApiResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RegisterUserCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }        public async Task<ApiResponse<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(request.UserName))
                return ApiResponse<string>.CreateError("Username is required");
            
            if (string.IsNullOrEmpty(request.Email))
                return ApiResponse<string>.CreateError("Email is required");
            
            if (string.IsNullOrEmpty(request.Password))
                return ApiResponse<string>.CreateError("Password is required");

            var user = new ApplicationUser { UserName = request.UserName, Email = request.Email };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return ApiResponse<string>.CreateError(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            return new ApiResponse<string>(user.Id) { Message = "User registered successfully" };
        }
    }
}
