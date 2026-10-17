using System.Data;
using System.Security.Cryptography;
using System.Text;
using Ritma.Api.Data;
using Ritma.Api.DTOs;
using Ritma.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Ritma.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public/booking")]
public sealed class PublicBookingController : ControllerBase
{
    private const int SlotIntervalMinutes = 15;
    private readonly RitmaDbContext _context;

    public PublicBookingController(RitmaDbContext context) => _context = context;

    [HttpGet("{businessId:int}")]
    public async Task<IActionResult> GetBusiness(int businessId)
    {
        var business = await _context.Businesses.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == businessId && b.IsActive && b.BookingEnabled);

        if (business == null)
        {
            return NotFound(new { message = "Bu işletme şu anda çevrimiçi randevu kabul etmiyor." });
        }

        var services = await _context.Services.AsNoTracking()
            .Where(s => s.BusinessId == businessId && s.IsActive).OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name, s.Description, s.DurationMinutes, s.Price }).ToListAsync();
        var staff = await _context.Staff.AsNoTracking()
            .Where(s => s.BusinessId == businessId && s.IsActive).OrderBy(s => s.FirstName).ThenBy(s => s.LastName)
            .Select(s => new { s.Id, s.FirstName, s.LastName }).ToListAsync();

        return Ok(new
        {
            business.Id, business.Name, business.Phone, business.Email, business.Address,
            openingTime = business.OpeningTime.ToString("HH:mm"),
            closingTime = business.ClosingTime.ToString("HH:mm"),
            business.WorkingDaysMask, services, staff
        });
    }

    [HttpGet("{businessId:int}/slots")]
    public async Task<IActionResult> GetSlots(int businessId, [FromQuery] int serviceId, [FromQuery] DateOnly date, [FromQuery] int? staffId = null)
    {
        if (date < DateOnly.FromDateTime(DateTime.Today) || date > DateOnly.FromDateTime(DateTime.Today.AddDays(60)))
        {
            return BadRequest(new { message = "Lütfen önümüzdeki 60 gün içinden bir tarih seç." });
        }

        var business = await _context.Businesses.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == businessId && b.IsActive && b.BookingEnabled);
        if (business == null)
        {
            return NotFound(new { message = "Bu işletme şu anda çevrimiçi randevu kabul etmiyor." });
        }

        var service = await _context.Services.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.BusinessId == businessId && s.IsActive);
        if (service == null)
        {
            return NotFound(new { message = "Hizmet bulunamadı." });
        }

        if (!IsOpenOn(business, date))
        {
            return Ok(Array.Empty<object>());
        }

        var staffQuery = _context.Staff.AsNoTracking()
            .Where(s => s.BusinessId == businessId && s.IsActive);
        if (staffId.HasValue)
        {
            staffQuery = staffQuery.Where(s => s.Id == staffId.Value);
        }

        var staff = await staffQuery.OrderBy(s => s.FirstName).ThenBy(s => s.LastName).ToListAsync();
        if (staff.Count == 0)
        {
            return Ok(Array.Empty<object>());
        }

        var dayStart = At(date, TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);
        var staffIds = staff.Select(s => s.Id).ToArray();
        var busy = await _context.Appointments.AsNoTracking()
            .Where(a => a.BusinessId == businessId && staffIds.Contains(a.StaffId) && a.Status != "Cancelled" && a.StartTime < dayEnd && a.EndTime > dayStart)
            .Select(a => new { a.StaffId, a.StartTime, a.EndTime })
            .ToListAsync();

        var slots = new List<object>();
        for (var start = At(date, business.OpeningTime); start.AddMinutes(service.DurationMinutes) <= At(date, business.ClosingTime); start = start.AddMinutes(SlotIntervalMinutes))
        {
            var end = start.AddMinutes(service.DurationMinutes);
            if (start <= DateTime.Now)
            {
                continue;
            }

            foreach (var member in staff)
            {
                if (!busy.Any(x => x.StaffId == member.Id && x.StartTime < end && x.EndTime > start))
                {
                    slots.Add(new
                    {
                        staffId = member.Id,
                        staffName = $"{member.FirstName} {member.LastName}",
                        startTime = start,
                        endTime = end
                    });
                }
            }
        }

        return Ok(slots);
    }

    [HttpPost("{businessId:int}")]
    [EnableRateLimiting("public-booking")]
    public async Task<IActionResult> Create(int businessId, PublicBookingRequestDto dto)
    {
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == businessId && b.IsActive && b.BookingEnabled);
        if (business == null)
        {
            return NotFound(new { message = "Bu işletme şu anda çevrimiçi randevu kabul etmiyor." });
        }

        var service = await _context.Services.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == dto.ServiceId && s.BusinessId == businessId && s.IsActive);
        if (service == null)
        {
            return BadRequest(new { message = "Lütfen geçerli bir hizmet seç." });
        }

        var start = DateTime.SpecifyKind(dto.StartTime, DateTimeKind.Unspecified);
        var end = start.AddMinutes(service.DurationMinutes);
        var date = DateOnly.FromDateTime(start);
        if (start <= DateTime.Now || start.Minute % SlotIntervalMinutes != 0 || !IsOpenOn(business, date) || start.TimeOfDay < business.OpeningTime.ToTimeSpan() || end.TimeOfDay > business.ClosingTime.ToTimeSpan())
        {
            return BadRequest(new { message = "Seçtiğin saat çalışma saatleri dışında veya artık uygun değil." });
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var staffQuery = _context.Staff.Where(s => s.BusinessId == businessId && s.IsActive);
        if (dto.StaffId.HasValue)
        {
            staffQuery = staffQuery.Where(s => s.Id == dto.StaffId.Value);
        }

        var staff = await staffQuery.OrderBy(s => s.FirstName).ThenBy(s => s.LastName).ToListAsync();
        if (staff.Count == 0)
        {
            return BadRequest(new { message = "Lütfen geçerli bir ekip üyesi seç." });
        }

        var selectedStaff = new List<Staff>();
        foreach (var member in staff)
        {
            var conflict = await _context.Appointments.AnyAsync(a =>
                a.BusinessId == businessId && a.StaffId == member.Id && a.Status != "Cancelled" && a.StartTime < end && a.EndTime > start);
            if (!conflict)
            {
                selectedStaff.Add(member);
                break;
            }
        }

        if (selectedStaff.Count == 0)
        {
            return Conflict(new { message = "Bu saat az önce doldu. Başka bir saat seçebilirsin." });
        }

        var phone = dto.Phone.Trim();
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.BusinessId == businessId && c.Phone == phone);
        if (customer == null)
        {
            customer = new Customer
            {
                BusinessId = businessId,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Phone = phone,
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim()
            };
            _context.Customers.Add(customer);
        }

        var manageToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var appointment = new Appointment
        {
            BusinessId = businessId,
            Customer = customer,
            StaffId = selectedStaff[0].Id,
            ServiceId = service.Id,
            StartTime = start,
            EndTime = end,
            Status = "Pending",
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            PublicManageTokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(manageToken)))
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Created($"/api/public/booking/{businessId}", new
        {
            appointmentId = appointment.Id,
            businessName = business.Name,
            appointment.StartTime,
            appointment.EndTime,
            appointment.Status,
            manageToken
        });
    }

    [HttpPost("{businessId:int}/cancel")]
    [EnableRateLimiting("public-booking")]
    public async Task<IActionResult> Cancel(int businessId, PublicBookingCancelDto dto)
    {
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(dto.ManageToken)));
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a =>
            a.BusinessId == businessId && a.PublicManageTokenHash == tokenHash);

        if (appointment == null)
        {
            return NotFound(new { message = "Randevu bağlantısı geçersiz." });
        }

        if (appointment.StartTime <= DateTime.Now || appointment.Status is "Cancelled" or "Completed")
        {
            return Conflict(new { message = "Bu randevu artık iptal edilemiyor. İşletmeyle iletişime geçebilirsin." });
        }

        appointment.Status = "Cancelled";
        appointment.PublicManageTokenHash = null;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Randevun iptal edildi." });
    }

    private static bool IsOpenOn(Business business, DateOnly date) =>
        (business.WorkingDaysMask & (1 << (int)date.DayOfWeek)) != 0;

    private static DateTime At(DateOnly date, TimeOnly time) =>
        DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);
}
