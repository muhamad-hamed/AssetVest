using AssetVest.Application.DTOs.Users;
using MediatR;

namespace AssetVest.Application.Queries.Users.GetUserById;

/// <summary>
/// Query to retrieve a user by their ID
/// </summary>
public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;
