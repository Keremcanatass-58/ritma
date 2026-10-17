using System.ComponentModel.DataAnnotations;

namespace Ritma.Api.DTOs;

public class RegisterUserDto
{
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

    [RegularExpression("^(Employee|Manager)$")]
    public string Role { get; set; } = "Employee";
}
