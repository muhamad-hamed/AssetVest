using AssetVest.Application.DTOs.Users;
using MediatR;

namespace AssetVest.Application.Queries.Users.GetCurrentUser;

/// <summary>
/// Query to retrieve the currently authenticated user
/// </summary>
public record GetCurrentUserQuery : IRequest<UserDto?>;
