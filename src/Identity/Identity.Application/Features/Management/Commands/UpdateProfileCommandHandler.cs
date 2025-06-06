using Identity.Application.Common.Models;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Application.Features.Management.Commands
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ApiResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UpdateProfileCommandHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }        public async Task<ApiResponse<string>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            if (userId == null)
                return ApiResponse<string>.CreateError("User not authenticated");
                
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiResponse<string>.CreateError("User not found");
                
            // Update user properties
            if (!string.IsNullOrEmpty(request.Email)) 
                user.Email = request.Email;
            if (!string.IsNullOrEmpty(request.FirstName)) 
                user.FirstName = request.FirstName;
            if (!string.IsNullOrEmpty(request.LastName)) 
                user.LastName = request.LastName;
            if (!string.IsNullOrEmpty(request.PreferredLanguage)) 
                user.PreferredLanguage = request.PreferredLanguage;
            if (!string.IsNullOrEmpty(request.TimeZone)) 
                user.TimeZone = request.TimeZone;
            if (request.EmailNotifications.HasValue) 
                user.EmailNotifications = request.EmailNotifications.Value;
            if (request.SmsNotifications.HasValue) 
                user.SmsNotifications = request.SmsNotifications.Value;
                
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return ApiResponse<string>.CreateError(string.Join(", ", result.Errors.Select(e => e.Description)));
                
            return new ApiResponse<string>(user.Id) { Message = "Profile updated successfully" };
        }
    }
}
