using AssetVest.Application.DTOs.Assets;
using MediatR;

namespace AssetVest.Application.Queries.Assets.GetAssetById;

/// <summary>
/// Query to retrieve an asset by its ID
/// </summary>
public record GetAssetByIdQuery(Guid AssetId) : IRequest<AssetDto?>;
