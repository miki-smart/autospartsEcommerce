using MediatR;
using Identity.Application.Common.Models;
using Identity.Application.Common.Mappings;
using System.Collections.Generic;

namespace Identity.Application.Features.Users.Queries;

public class GetLoginHistoryQuery : IRequest<ApiResponse<List<LoginHistoryDto>>>
{
    public string? UserId { get; set; } // If null, use current user
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
