using AssetVest.Application.DTOs.Users;
using MediatR;

namespace AssetVest.Application.Commands.Users.CreateUser;

/// <summary>
/// Command to create a new user in the system
/// </summary>
public record CreateUserCommand : IRequest<UserDto>
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}
