using AssetVest.Application.DTOs.Users;
using MediatR;

namespace AssetVest.Application.Commands.Users.UpdateUser;

/// <summary>
/// Command to update an existing user's information
/// </summary>
public record UpdateUserCommand : IRequest<UserDto?>
{
    public required Guid UserId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
}
