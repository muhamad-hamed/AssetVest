using MediatR;

namespace AssetVest.Application.Commands.Assets.DeleteAsset;

/// <summary>
/// Command to delete an asset (soft delete)
/// </summary>
public record DeleteAssetCommand(Guid AssetId) : IRequest<bool>;
