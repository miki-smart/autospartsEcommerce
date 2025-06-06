using AutoMapper;
using Identity.Application.Common.DTOs;
using Identity.Domain.Entities;

namespace Identity.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Roles, opt => opt.Ignore())
            .ForMember(dest => dest.Permissions, opt => opt.Ignore());

        CreateMap<CreateUserDto, ApplicationUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => false));

        // Role mappings
        CreateMap<ApplicationRole, RoleDto>()
            .ForMember(dest => dest.Permissions, opt => opt.Ignore());

        CreateMap<CreateRoleDto, ApplicationRole>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // Permission mappings
        CreateMap<Permission, PermissionDto>();

        // Device mappings
        CreateMap<Device, DeviceDto>();

        // Login history mappings
        CreateMap<LoginHistory, LoginHistoryDto>();
    }
}

// Additional DTOs for device and login history
public class DeviceDto
{
    public int Id { get; set; }
    public string DeviceIdentifier { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? Platform { get; set; }
    public string? DeviceType { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Browser { get; set; }
    public bool IsTrusted { get; set; }
    public DateTime FirstSeenDate { get; set; }
    public DateTime LastSeenDate { get; set; }
    public string? LastIpAddress { get; set; }
    public bool IsActive { get; set; }
}

public class LoginHistoryDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? Platform { get; set; }
    public string? Browser { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public DeviceDto? Device { get; set; }
}
