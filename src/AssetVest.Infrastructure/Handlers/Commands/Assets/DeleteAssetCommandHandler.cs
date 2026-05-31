using AssetVest.Application.Commands.Assets.DeleteAsset;
using AssetVest.Application.Ports;
using AssetVest.Infrastructure.Persistence;
using MediatR;

namespace AssetVest.Infrastructure.Handlers.Commands.Assets;

public class DeleteAssetCommandHandler(ApplicationDbContext context, ICurrentUserService currentUserService) 
    : IRequestHandler<DeleteAssetCommand, bool>
{
    public async Task<bool> Handle(DeleteAssetCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        var asset = await context.Assets.FindAsync([request.AssetId], cancellationToken);

        if (asset is null || asset.UserId != userId)
            return false;

        context.Assets.Remove(asset); // Soft delete handled by DbContext
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
