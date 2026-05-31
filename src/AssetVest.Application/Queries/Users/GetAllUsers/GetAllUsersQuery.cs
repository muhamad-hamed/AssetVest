using AssetVest.Application.DTOs.Users;
using MediatR;

namespace AssetVest.Application.Queries.Users.GetAllUsers;

/// <summary>
/// Query to retrieve all users
/// </summary>
public record GetAllUsersQuery : IRequest<IReadOnlyList<UserDto>>;
