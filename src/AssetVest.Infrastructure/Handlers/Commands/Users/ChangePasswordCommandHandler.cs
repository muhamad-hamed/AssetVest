using AssetVest.Application.Commands.Users.ChangePassword;
using AssetVest.Infrastructure.Persistence;
using MediatR;

namespace AssetVest.Infrastructure.Handlers.Commands.Users;

public class ChangePasswordCommandHandler(ApplicationDbContext context) : IRequestHandler<ChangePasswordCommand, bool>
{
    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync([request.UserId], cancellationToken);

        if (user is null)
            return false;

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
