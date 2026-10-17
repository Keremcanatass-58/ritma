using System.ComponentModel.DataAnnotations;

namespace Ritma.Api.Models;

public class Appointment
{
    public int Id { get; set; }

    public int BusinessId { get; set; }

    public int CustomerId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(64)]
    public string? PublicManageTokenHash { get; set; }

    public int StaffId { get; set; }

    public int ServiceId { get; set; }

    public Customer Customer { get; set; } = null!;

    public Business Business { get; set; } = null!;

    public Staff Staff { get; set; } = null!;

    public Service Service { get; set; } = null!;
}
