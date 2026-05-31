using AssetVest.Application.DTOs.Users;
using AssetVest.Application.Ports;
using AssetVest.Application.Queries.Users.GetCurrentUser;
using AssetVest.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssetVest.Infrastructure.Handlers.Queries.Users;

public class GetCurrentUserQueryHandler(ApplicationDbContext context, ICurrentUserService currentUserService) 
    : IRequestHandler<GetCurrentUserQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (userId is null)
            return null;

        var user = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId.Value)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }
}
