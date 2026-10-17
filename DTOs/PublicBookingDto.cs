using System.ComponentModel.DataAnnotations;

namespace Ritma.Api.DTOs;

public class PublicBookingRequestDto
{
    [Range(1, int.MaxValue)]
    public int ServiceId { get; set; }

    public int? StaffId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required, Phone, StringLength(20, MinimumLength = 7)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class PublicBookingCancelDto
{
    [Required, StringLength(128, MinimumLength = 32)]
    public string ManageToken { get; set; } = string.Empty;
}
