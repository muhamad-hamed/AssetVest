using Asp.Versioning;
using AssetVest.Application.Commands.AnnualGoals.CreateAnnualGoal;
using AssetVest.Application.Commands.AnnualGoals.DeleteAnnualGoal;
using AssetVest.Application.Commands.AnnualGoals.UpdateAnnualGoal;
using AssetVest.Application.DTOs.AnnualGoals;
using AssetVest.Application.Queries.AnnualGoals.GetAllAnnualGoals;
using AssetVest.Application.Queries.AnnualGoals.GetAnnualGoalById;
using AssetVest.Application.Queries.AnnualGoals.GetAnnualGoalByYear;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetVest.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/annual-goals")]
[Authorize]
public class AnnualGoalsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Get all annual goals for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AnnualGoalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AnnualGoalDto>>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllAnnualGoalsQuery();
        var goals = await sender.Send(query, cancellationToken);
        return Ok(goals);
    }

    /// <summary>
    /// Get annual goal by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AnnualGoalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnnualGoalDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAnnualGoalByIdQuery(id);
        var goal = await sender.Send(query, cancellationToken);

        if (goal is null)
            return NotFound();

        return Ok(goal);
    }

    /// <summary>
    /// Get annual goal by year
    /// </summary>
    [HttpGet("by-year/{year:int}")]
    [ProducesResponseType(typeof(AnnualGoalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnnualGoalDto>> GetByYear(int year, CancellationToken cancellationToken)
    {
        var query = new GetAnnualGoalByYearQuery(year);
        var goal = await sender.Send(query, cancellationToken);

        if (goal is null)
            return NotFound();

        return Ok(goal);
    }

    /// <summary>
    /// Create a new annual goal
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AnnualGoalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AnnualGoalDto>> Create([FromBody] CreateAnnualGoalRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateAnnualGoalCommand
            {
                Year = request.Year,
                TargetTotalPortfolioValueEGP = request.TargetTotalPortfolioValueEGP,
                TargetProfitPercent = request.TargetProfitPercent,
                Notes = request.Notes,
                AllocationGoals = request.AllocationGoals
            };

            var goal = await sender.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = goal.Id }, goal);
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
    /// Update an existing annual goal
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AnnualGoalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AnnualGoalDto>> Update(Guid id, [FromBody] UpdateAnnualGoalRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateAnnualGoalCommand
            {
                Id = id,
                TargetTotalPortfolioValueEGP = request.TargetTotalPortfolioValueEGP,
                TargetProfitPercent = request.TargetProfitPercent,
                Notes = request.Notes,
                AllocationGoals = request.AllocationGoals
            };

            var goal = await sender.Send(command, cancellationToken);

            if (goal is null)
                return NotFound();

            return Ok(goal);
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
    /// Delete an annual goal
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteAnnualGoalCommand(id);
        var deleted = await sender.Send(command, cancellationToken);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
