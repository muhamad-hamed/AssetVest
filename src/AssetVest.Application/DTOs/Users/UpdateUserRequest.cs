using System.ComponentModel.DataAnnotations;

namespace AssetVest.Application.DTOs.Users;

public record UpdateUserRequest
{
    [Required]
    [MaxLength(100)]
    public required string FirstName { get; init; }

    [Required]
    [MaxLength(100)]
    public required string LastName { get; init; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public required string Email { get; init; }
}
