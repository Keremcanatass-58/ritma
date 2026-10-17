using System.ComponentModel.DataAnnotations;

namespace Ritma.Api.Models;

public class Staff
{
    public int Id { get; set; }

    public int BusinessId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "Employee";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Business Business { get; set; } = null!;
}