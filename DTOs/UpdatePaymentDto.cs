using System.ComponentModel.DataAnnotations;

namespace Ritma.Api.DTOs;

public class UpdatePaymentDto
{

    [Required]
    [RegularExpression("^(Paid|Pending|Refunded|Cancelled)$", ErrorMessage = "Status must be Paid, Pending, Refunded or Cancelled.")]
    public string Status { get; set; } = "Paid";

    [StringLength(500)]
    public string? Notes { get; set; }
}
