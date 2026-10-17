using System.ComponentModel.DataAnnotations;

namespace Ritma.Api.DTOs;

public class CreateServiceDto
{

    [Required(ErrorMessage = "Service name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes.")]
    public int DurationMinutes { get; set; }

    [Range(0, 999999999, ErrorMessage = "Price must be a valid amount.")]
    public decimal Price { get; set; }
}
