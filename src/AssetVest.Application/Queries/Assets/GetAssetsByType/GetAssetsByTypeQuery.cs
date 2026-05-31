using AssetVest.Application.DTOs.Assets;
using AssetVest.Domain.Enums;
using MediatR;

namespace AssetVest.Application.Queries.Assets.GetAssetsByType;

/// <summary>
/// Query to retrieve assets by type for the current user
/// </summary>
public record GetAssetsByTypeQuery(AssetType AssetType) : IRequest<IReadOnlyList<AssetDto>>;
