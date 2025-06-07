using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Identity.Application.Common.Models;
using Identity.Application.Common.DTOs;
using Identity.Application.Features.Users.Queries;
using Identity.Domain.Entities;
using AutoMapper;

namespace Identity.Application.Features.Users.Handlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        ILogger<GetUserByIdQueryHandler> logger)
    {
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", request.Id);
                return new ApiResponse<UserDto>("User not found");
            }

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

            return new ApiResponse<UserDto>(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", request.Id);
            return new ApiResponse<UserDto>("An error occurred while retrieving the user");
        }
    }
}

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, ApiResponse<List<UserDto>>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        ILogger<GetUsersQueryHandler> logger)
    {
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<List<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _userManager.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(u => 
                    u.Email!.ToLower().Contains(searchTerm) ||
                    u.UserName!.ToLower().Contains(searchTerm) ||
                    u.FirstName!.ToLower().Contains(searchTerm) ||
                    u.LastName!.ToLower().Contains(searchTerm));
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == request.IsActive.Value);
            }

            // Apply pagination
            var users = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                userDtos.Add(userDto);
            }

            // Filter by role if specified
            if (!string.IsNullOrEmpty(request.Role))
            {
                userDtos = userDtos.Where(u => u.Roles.Contains(request.Role)).ToList();
            }

            return new ApiResponse<List<UserDto>>(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return new ApiResponse<List<UserDto>>("An error occurred while retrieving users");
        }
    }
}
