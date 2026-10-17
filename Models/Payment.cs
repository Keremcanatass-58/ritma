using System.ComponentModel.DataAnnotations;

namespace Ritma.Api.Models;

public class Payment
{
    public int Id { get; set; }

    public int BusinessId { get; set; }

    public int CustomerId { get; set; }

    public int AppointmentId { get; set; }

    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = "Cash";

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Paid";

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Business Business { get; set; } = null!;

    public Customer Customer { get; set; } = null!;

    public Appointment Appointment { get; set; } = null!;
}
