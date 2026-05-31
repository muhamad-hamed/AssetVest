using AssetVest.Application.DTOs.Assets;
using MediatR;

namespace AssetVest.Application.Queries.Assets.GetAllAssets;

/// <summary>
/// Query to retrieve all assets for the current user
/// </summary>
public record GetAllAssetsQuery : IRequest<IReadOnlyList<AssetDto>>;
