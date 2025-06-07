using MediatR;
using Identity.Application.Common.Models;

namespace Identity.Application.Features.Management.Queries
{
    public class GetProfileQuery : IRequest<ApiResponse<UserProfileDto>>
    {
    }    public class UserProfileDto
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? TimeZone { get; set; }
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; }
    }
}
