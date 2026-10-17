using System.ComponentModel.DataAnnotations;

namespace Ritma.Api.DTOs;

public class RegisterOwnerDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string BusinessName { get; set; } = string.Empty;

    [Phone]
    public string? BusinessPhone { get; set; }

    [EmailAddress]
    public string? BusinessEmail { get; set; }

    [StringLength(250)]
    public string? BusinessAddress { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}
