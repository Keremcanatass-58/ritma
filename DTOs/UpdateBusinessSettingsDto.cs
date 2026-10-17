using System.ComponentModel.DataAnnotations;

namespace Ritma.Api.DTOs;

public class UpdateBusinessSettingsDto
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Phone, StringLength(20)]
    public string? Phone { get; set; }

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    public TimeOnly OpeningTime { get; set; }

    public TimeOnly ClosingTime { get; set; }

    [Range(1, 127)]
    public byte WorkingDaysMask { get; set; }

    public bool BookingEnabled { get; set; }
}
