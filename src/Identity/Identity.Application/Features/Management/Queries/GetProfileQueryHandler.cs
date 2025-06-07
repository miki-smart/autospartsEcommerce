using Identity.Application.Common.Models;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Application.Features.Management.Queries
{
    public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ApiResponse<UserProfileDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public GetProfileQueryHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ApiResponse<UserProfileDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            if (userId == null)
                return ApiResponse<UserProfileDto>.CreateError("User not authenticated");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiResponse<UserProfileDto>.CreateError("User not found");            var dto = new UserProfileDto
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                PreferredLanguage = user.PreferredLanguage,
                TimeZone = user.TimeZone,
                EmailNotifications = user.EmailNotifications,
                SmsNotifications = user.SmsNotifications,
                ProfilePicture = user.ProfilePicture,
                LastLoginDate = user.LastLoginDate,
                IsActive = user.IsActive
            };
            return new ApiResponse<UserProfileDto>(dto);
        }
    }
}
