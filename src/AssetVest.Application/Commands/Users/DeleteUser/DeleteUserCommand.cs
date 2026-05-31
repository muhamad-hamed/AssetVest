using MediatR;

namespace AssetVest.Application.Commands.Users.DeleteUser;

/// <summary>
/// Command to delete a user (soft delete)
/// </summary>
public record DeleteUserCommand(Guid UserId) : IRequest<bool>;
