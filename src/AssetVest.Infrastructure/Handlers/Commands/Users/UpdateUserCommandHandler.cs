using AssetVest.Application.Commands.Users.UpdateUser;
using AssetVest.Application.DTOs.Users;
using AssetVest.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssetVest.Infrastructure.Handlers.Commands.Users;

public class UpdateUserCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateUserCommand, UserDto?>
{
    public async Task<UserDto?> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync([request.UserId], cancellationToken);

        if (user is null)
            return null;

        // Check if email is being changed and if it's already taken
        if (user.Email != request.Email)
        {
            var emailExists = await context.Users
                .AnyAsync(u => u.Email == request.Email && u.Id != request.UserId, cancellationToken);

            if (emailExists)
            {
                throw new InvalidOperationException($"A user with email '{request.Email}' already exists.");
            }
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;

        await context.SaveChangesAsync(cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
