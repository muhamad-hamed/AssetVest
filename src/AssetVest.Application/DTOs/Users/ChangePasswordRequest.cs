using System.ComponentModel.DataAnnotations;

namespace AssetVest.Application.DTOs.Users;

public record ChangePasswordRequest
{
    [Required]
    public required string CurrentPassword { get; init; }

    [Required]
    [MinLength(8)]
    public required string NewPassword { get; init; }
}
