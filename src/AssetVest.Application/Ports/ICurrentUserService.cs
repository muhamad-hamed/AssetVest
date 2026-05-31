namespace AssetVest.Application.Ports;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? IpAddress { get; }
}
