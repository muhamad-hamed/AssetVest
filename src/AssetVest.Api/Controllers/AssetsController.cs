using Asp.Versioning;
using AssetVest.Application.Commands.Assets.CreateAsset;
using AssetVest.Application.Commands.Assets.DeleteAsset;
using AssetVest.Application.Commands.Assets.UpdateAsset;
using AssetVest.Application.DTOs.Assets;
using AssetVest.Application.Queries.Assets.GetAllAssets;
using AssetVest.Application.Queries.Assets.GetAssetById;
using AssetVest.Application.Queries.Assets.GetAssetsByType;
using AssetVest.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetVest.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class AssetsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Get all assets for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AssetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AssetDto>>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllAssetsQuery();
        var assets = await sender.Send(query, cancellationToken);
        return Ok(assets);
    }

    /// <summary>
    /// Get asset by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAssetByIdQuery(id);
        var asset = await sender.Send(query, cancellationToken);

        if (asset is null)
            return NotFound();

        return Ok(asset);
    }

    /// <summary>
    /// Get assets by type
    /// </summary>
    [HttpGet("by-type/{assetType}")]
    [ProducesResponseType(typeof(IReadOnlyList<AssetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AssetDto>>> GetByType(AssetType assetType, CancellationToken cancellationToken)
    {
        var query = new GetAssetsByTypeQuery(assetType);
        var assets = await sender.Send(query, cancellationToken);
        return Ok(assets);
    }

    /// <summary>
    /// Create a new asset
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AssetDto>> Create([FromBody] CreateAssetRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateAssetCommand
            {
                Name = request.Name,
                AssetType = request.AssetType,
                BaseCurrency = request.BaseCurrency,
                InitialValueEGP = request.InitialValueEGP,
                CurrentValueEGP = request.CurrentValueEGP,
                Notes = request.Notes,
                // Detail objects (just pass through)
                StockDetail = request.StockDetail,
                CurrencyDetail = request.CurrencyDetail,
                GoldDetail = request.GoldDetail,
                RealEstateDetail = request.RealEstateDetail,
                MutualFundDetail = request.MutualFundDetail,
                CryptoDetail = request.CryptoDetail,
                BondsDetail = request.BondsDetail
            };

            var asset = await sender.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = asset.Id }, asset);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized
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
    /// Update an existing asset
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AssetDto>> Update(Guid id, [FromBody] UpdateAssetRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateAssetCommand
            {
                AssetId = id,
                Name = request.Name,
                AssetType = request.AssetType,
                BaseCurrency = request.BaseCurrency,
                CurrentValueEGP = request.CurrentValueEGP,
                Notes = request.Notes,
                // Detail objects (just pass through)
                StockDetail = request.StockDetail,
                CurrencyDetail = request.CurrencyDetail,
                GoldDetail = request.GoldDetail,
                RealEstateDetail = request.RealEstateDetail,
                MutualFundDetail = request.MutualFundDetail,
                CryptoDetail = request.CryptoDetail,
                BondsDetail = request.BondsDetail
            };

            var asset = await sender.Send(command, cancellationToken);

            if (asset is null)
                return NotFound();

            return Ok(asset);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized
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
    /// Delete an asset (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteAssetCommand(id);
            var deleted = await sender.Send(command, cancellationToken);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized
            });
        }
    }
}
