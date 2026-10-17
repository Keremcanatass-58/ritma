using System.ComponentModel.DataAnnotations;

namespace Ritma.Api.DTOs;

public class CreateBusinessDto
{
    [Required(ErrorMessage = "Business name is required.")]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number.")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string? Email { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }
}