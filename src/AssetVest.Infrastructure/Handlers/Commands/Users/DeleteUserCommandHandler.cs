using AssetVest.Application.Commands.Users.DeleteUser;
using AssetVest.Infrastructure.Persistence;
using MediatR;

namespace AssetVest.Infrastructure.Handlers.Commands.Users;

public class DeleteUserCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteUserCommand, bool>
{
    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync([request.UserId], cancellationToken);

        if (user is null)
            return false;

        context.Users.Remove(user); // Soft delete handled by DbContext
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
