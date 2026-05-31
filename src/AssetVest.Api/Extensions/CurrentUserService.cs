using System.Security.Claims;
using AssetVest.Application.Ports;

namespace AssetVest.Api.Extensions;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService> logger) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null || user.Identity?.IsAuthenticated != true)
            {
                logger.LogDebug("User is not authenticated");
                return null;
            }

            // With claim mapping disabled, we use the original "sub" claim
            var value = user.FindFirst("sub")?.Value;

            if (value == null)
            {
                logger.LogWarning("Could not find 'sub' claim in token. Available claims: {Claims}", 
                    string.Join(", ", user.Claims.Select(c => c.Type)));
                return null;
            }

            if (!Guid.TryParse(value, out var id))
            {
                logger.LogWarning("'sub' claim value is not a valid GUID: {Value}", value);
                return null;
            }

            logger.LogDebug("Extracted User ID from 'sub' claim: {UserId}", id);
            return id;
        }
    }

    public string? IpAddress =>
        httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
}
