using Asp.Versioning;
using AssetVest.Application.Commands.Users.ChangePassword;
using AssetVest.Application.Commands.Users.CreateUser;
using AssetVest.Application.Commands.Users.DeleteUser;
using AssetVest.Application.Commands.Users.ToggleActiveStatus;
using AssetVest.Application.Commands.Users.UpdateUser;
using AssetVest.Application.DTOs.Users;
using AssetVest.Application.Ports;
using AssetVest.Application.Queries.Users.GetCurrentUser;
using AssetVest.Application.Queries.Users.GetUserById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetVest.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class UsersController(ISender sender, ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>
    /// Get user by ID. Callers may only read their own record.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (currentUser.UserId != id)
            return Forbid();

        var query = new GetUserByIdQuery(id);
        var user = await sender.Send(query, cancellationToken);

        if (user is null)
            return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Get current authenticated user
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var query = new GetCurrentUserQuery();
        var user = await sender.Send(query, cancellationToken);

        if (user is null)
            return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateUserCommand
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password
            };

            var user = await sender.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)),
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Update the authenticated user's profile
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> UpdateCurrentUser([FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
            return Unauthorized();

        return await UpdateUserAsync(userId, request, cancellationToken);
    }

    /// <summary>
    /// Update an existing user. Callers may only update their own record.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId != id)
            return Forbid();

        return await UpdateUserAsync(id, request, cancellationToken);
    }

    private async Task<ActionResult<UserDto>> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateUserCommand
            {
                UserId = userId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            };

            var user = await sender.Send(command, cancellationToken);

            if (user is null)
                return NotFound();

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)),
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Delete a user (soft delete). Callers may only delete their own record.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (currentUser.UserId != id)
            return Forbid();

        var command = new DeleteUserCommand(id);
        var deleted = await sender.Send(command, cancellationToken);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Change the authenticated user's password
    /// </summary>
    [HttpPost("me/change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangeCurrentUserPassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
            return Unauthorized();

        return await ChangePasswordAsync(userId, request, cancellationToken);
    }

    /// <summary>
    /// Change user password. Callers may only change their own password.
    /// </summary>
    [HttpPost("{id:guid}/change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId != id)
            return Forbid();

        return await ChangePasswordAsync(id, request, cancellationToken);
    }

    private async Task<IActionResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ChangePasswordCommand
            {
                UserId = userId,
                CurrentPassword = request.CurrentPassword,
                NewPassword = request.NewPassword
            };

            var success = await sender.Send(command, cancellationToken);

            if (!success)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)),
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Toggle user active status. Callers may only change their own status.
    /// </summary>
    [HttpPost("{id:guid}/toggle-active")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken cancellationToken)
    {
        if (currentUser.UserId != id)
            return Forbid();

        var command = new ToggleUserActiveStatusCommand(id);
        var success = await sender.Send(command, cancellationToken);

        if (!success)
            return NotFound();

        return NoContent();
    }
}
