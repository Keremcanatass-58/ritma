using System.ComponentModel.DataAnnotations;

namespace Ritma.Api.DTOs;

public class CreateAppointmentDto
{
    [Range(1, int.MaxValue)]
    public int ServiceId { get; set; }
    [Range(1, int.MaxValue)]
    public int StaffId { get; set; }

    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [RegularExpression("^(Pending|Confirmed|Completed|Cancelled)$")]
    public string Status { get; set; } = "Pending";

    [StringLength(500)]
    public string? Notes { get; set; }
}
