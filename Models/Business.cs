using System.ComponentModel.DataAnnotations;

namespace Ritma.Api.Models;

public class Business
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(256)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public bool BookingEnabled { get; set; }

    public TimeOnly OpeningTime { get; set; } = new(9, 0);

    public TimeOnly ClosingTime { get; set; } = new(18, 0);

    // Bits follow System.DayOfWeek: Sunday is bit 0, Saturday is bit 6.
    public byte WorkingDaysMask { get; set; } = 126;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Staff> Staff { get; set; } = new List<Staff>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
