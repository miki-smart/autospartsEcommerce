using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Identity.Application.Common.Models;
using Identity.Application.Common.Mappings;
using Identity.Application.Features.Users.Queries;
using Identity.Domain.Entities;
using AutoMapper;

namespace Identity.Application.Features.Users.Handlers;

public class GetLoginHistoryQueryHandler : IRequestHandler<GetLoginHistoryQuery, ApiResponse<List<LoginHistoryDto>>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<GetLoginHistoryQueryHandler> _logger;

    public GetLoginHistoryQueryHandler(
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        ILogger<GetLoginHistoryQueryHandler> logger)
    {
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<List<LoginHistoryDto>>> Handle(GetLoginHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            ApplicationUser? user = null;
            if (!string.IsNullOrEmpty(request.UserId))
            {
                user = await _userManager.Users
                    .Include(u => u.LoginHistories)
                    .ThenInclude(lh => lh.Device)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            }
            else
            {
                // Should be set by controller if not provided
                return new ApiResponse<List<LoginHistoryDto>>("UserId is required");
            }

            if (user == null)
            {
                _logger.LogWarning("User not found for login history: {UserId}", request.UserId);
                return new ApiResponse<List<LoginHistoryDto>>("User not found");
            }

            var loginHistories = user.LoginHistories
                .OrderByDescending(lh => lh.LoginTime)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<LoginHistoryDto>>(loginHistories);
            return new ApiResponse<List<LoginHistoryDto>>(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting login history for user: {UserId}", request.UserId);
            return new ApiResponse<List<LoginHistoryDto>>("An error occurred while retrieving login history");
        }
    }
}
