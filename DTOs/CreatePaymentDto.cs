using System.ComponentModel.DataAnnotations;

namespace Ritma.Api.DTOs;

public class CreatePaymentDto
{

    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue)]
    public int AppointmentId { get; set; }

    [Range(0.01, 999999999)]
    public decimal Amount { get; set; }

    [Required]
    [RegularExpression("^(Cash|Card|Transfer)$", ErrorMessage = "Payment method must be Cash, Card or Transfer.")]
    public string PaymentMethod { get; set; } = "Cash";

    [Required]
    [RegularExpression("^(Paid|Pending|Refunded|Cancelled)$", ErrorMessage = "Status must be Paid, Pending, Refunded or Cancelled.")]
    public string Status { get; set; } = "Paid";

    [StringLength(500)]
    public string? Notes { get; set; }
}
