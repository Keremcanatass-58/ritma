using System.ComponentModel.DataAnnotations;

namespace Ritma.Api.DTOs;

public class UpdateStaffDto
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number.")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string? Email { get; set; }

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "Employee";

    public bool IsActive { get; set; } = true;
}
