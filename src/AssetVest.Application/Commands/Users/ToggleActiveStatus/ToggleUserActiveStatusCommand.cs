using MediatR;

namespace AssetVest.Application.Commands.Users.ToggleActiveStatus;

/// <summary>
/// Command to toggle a user's active status
/// </summary>
public record ToggleUserActiveStatusCommand(Guid UserId) : IRequest<bool>;
