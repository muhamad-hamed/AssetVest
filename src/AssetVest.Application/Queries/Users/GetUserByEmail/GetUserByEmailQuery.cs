using AssetVest.Application.DTOs.Users;
using MediatR;

namespace AssetVest.Application.Queries.Users.GetUserByEmail;

/// <summary>
/// Query to retrieve a user by their email address
/// </summary>
public record GetUserByEmailQuery(string Email) : IRequest<UserDto?>;
