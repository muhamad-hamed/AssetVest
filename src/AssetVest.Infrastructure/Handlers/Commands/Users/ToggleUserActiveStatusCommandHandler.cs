using AssetVest.Application.Commands.Users.ToggleActiveStatus;
using AssetVest.Infrastructure.Persistence;
using MediatR;

namespace AssetVest.Infrastructure.Handlers.Commands.Users;

public class ToggleUserActiveStatusCommandHandler(ApplicationDbContext context) : IRequestHandler<ToggleUserActiveStatusCommand, bool>
{
    public async Task<bool> Handle(ToggleUserActiveStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync([request.UserId], cancellationToken);

        if (user is null)
            return false;

        user.IsActive = !user.IsActive;
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
