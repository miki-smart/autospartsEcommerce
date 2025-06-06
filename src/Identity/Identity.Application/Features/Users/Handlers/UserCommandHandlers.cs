                                                                                          using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Identity.Application.Common.Models;
using Identity.Application.Common.DTOs;
using Identity.Application.Features.Users.Commands;
using Identity.Domain.Entities;
using AutoMapper;

namespace Identity.Application.Features.Users.Handlers;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ApiResponse<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating new user with email: {Email}", request.Email);

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("User creation failed - email already exists: {Email}", request.Email);
                return new ApiResponse<UserDto>("Email already exists");
            }

            existingUser = await _userManager.FindByNameAsync(request.UserName);
            if (existingUser != null)
            {
                _logger.LogWarning("User creation failed - username already exists: {UserName}", request.UserName);
                return new ApiResponse<UserDto>("Username already exists");
            }

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = false,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("User creation failed for email {Email}: {Errors}", request.Email, string.Join(", ", errors));
                return new ApiResponse<UserDto>(errors);
            }

            // Assign roles if provided
            if (request.Roles.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(user, request.Roles);
                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("Role assignment failed for user {UserId}: {Errors}", 
                        user.Id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = request.Roles;

            _logger.LogInformation("User created successfully: {UserId}", user.Id);
            return new ApiResponse<UserDto>(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email: {Email}", request.Email);
            return new ApiResponse<UserDto>("An error occurred while creating the user");
        }
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ApiResponse<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating user: {UserId}", request.Id);

            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", request.Id);
                return new ApiResponse<UserDto>("User not found");
            }

            // Update user properties
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.IsActive = request.IsActive;
            user.TwoFactorEnabled = request.TwoFactorEnabled;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("User update failed for {UserId}: {Errors}", request.Id, string.Join(", ", errors));
                return new ApiResponse<UserDto>(errors);
            }

            // Update roles if provided
            if (request.Roles.Any())
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (removeResult.Succeeded)
                {
                    var addResult = await _userManager.AddToRolesAsync(user, request.Roles);
                    if (!addResult.Succeeded)
                    {
                        _logger.LogWarning("Role update failed for user {UserId}: {Errors}", 
                            user.Id, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    }
                }
            }

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = request.Roles;

            _logger.LogInformation("User updated successfully: {UserId}", user.Id);
            return new ApiResponse<UserDto>(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", request.Id);
            return new ApiResponse<UserDto>("An error occurred while updating the user");
        }
    }
}
