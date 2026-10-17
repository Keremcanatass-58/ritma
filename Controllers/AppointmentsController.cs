using Ritma.Api.Data;
using Ritma.Api.DTOs;
using Ritma.Api.Models;
using Ritma.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Ritma.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AppointmentsController : BusinessScopedController
{
    private readonly RitmaDbContext _context;

    public AppointmentsController(RitmaDbContext context, ICurrentUser currentUser)
        : base(currentUser)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAppointmentDto dto)
    {
        var businessId = GetBusinessId();

        var business = await _context.Businesses.FindAsync(businessId);

        if (business == null)
        {
            return NotFound(new
            {
                message = "Business not found"
            });
        }

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c =>
                c.Id == dto.CustomerId &&
                c.BusinessId == businessId);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found for this business"
            });
        }

        var staff = await _context.Staff
            .FirstOrDefaultAsync(s =>
                s.Id == dto.StaffId &&
                s.BusinessId == businessId &&
                s.IsActive);

        if (staff == null)
        {
            return NotFound(new
            {
                message = "Active staff not found for this business"
            });
        }

        var service = await _context.Services
            .FirstOrDefaultAsync(s =>
                s.Id == dto.ServiceId &&
                s.BusinessId == businessId &&
                s.IsActive);

        if (service == null)
        {
            return NotFound(new
            {
                message = "Active service not found for this business"
            });
        }

        if (dto.EndTime <= dto.StartTime)
        {
            return BadRequest(new
            {
                message = "End time must be after start time."
            });
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var hasConflict = await _context.Appointments
            .AnyAsync(a =>
                a.BusinessId == businessId &&
                a.StaffId == dto.StaffId &&
                a.Status != "Cancelled" &&
                a.StartTime < dto.EndTime &&
                a.EndTime > dto.StartTime);

        if (hasConflict)
        {
            return Conflict(new
            {
                message = "Selected time slot is already booked for this staff."
            });
        }

        var appointment = new Appointment
        {
            BusinessId = businessId,
            CustomerId = dto.CustomerId,
            StaffId = dto.StaffId,
            ServiceId = dto.ServiceId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = dto.Status,
            Notes = dto.Notes
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(appointment);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20)
    {
        var businessId = GetBusinessId();

        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.Service)
            .Where(a => a.BusinessId == businessId);

        if (startDate.HasValue)
        {
            query = query.Where(a => a.StartTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.StartTime <= endDate.Value);
        }

        query = query.OrderByDescending(a => a.StartTime);

        var totalItems = await query.CountAsync();
        var appointments = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { items = appointments, totalItems, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var businessId = GetBusinessId();

        var appointment = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a =>
                a.Id == id &&
                a.BusinessId == businessId);

        if (appointment == null)
        {
            return NotFound(new
            {
                message = "Appointment not found for this business"
            });
        }

        return Ok(appointment);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateAppointmentDto dto)
    {
        var businessId = GetBusinessId();

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a =>
                a.Id == id &&
                a.BusinessId == businessId);

        if (appointment == null)
        {
            return NotFound(new
            {
                message = "Appointment not found for this business"
            });
        }

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c =>
                c.Id == dto.CustomerId &&
                c.BusinessId == businessId);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found for this business"
            });
        }

        var staff = await _context.Staff
            .FirstOrDefaultAsync(s =>
                s.Id == dto.StaffId &&
                s.BusinessId == businessId &&
                s.IsActive);

        if (staff == null)
        {
            return NotFound(new
            {
                message = "Active staff not found for this business"
            });
        }

        var service = await _context.Services
            .FirstOrDefaultAsync(s =>
                s.Id == dto.ServiceId &&
                s.BusinessId == businessId &&
                s.IsActive);

        if (service == null)
        {
            return NotFound(new
            {
                message = "Active service not found for this business"
            });
        }

        if (dto.EndTime <= dto.StartTime)
        {
            return BadRequest(new
            {
                message = "End time must be after start time."
            });
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var hasConflict = await _context.Appointments
            .AnyAsync(a =>
                a.Id != id &&
                a.BusinessId == businessId &&
                a.StaffId == dto.StaffId &&
                a.Status != "Cancelled" &&
                a.StartTime < dto.EndTime &&
                a.EndTime > dto.StartTime);

        if (hasConflict)
        {
            return Conflict(new
            {
                message = "Selected time slot is already booked for this staff."
            });
        }

        appointment.CustomerId = dto.CustomerId;
        appointment.StaffId = dto.StaffId;
        appointment.ServiceId = dto.ServiceId;
        appointment.StartTime = dto.StartTime;
        appointment.EndTime = dto.EndTime;
        appointment.Status = dto.Status;
        appointment.Notes = dto.Notes;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(appointment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var businessId = GetBusinessId();

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id && a.BusinessId == businessId);

        if (appointment == null)
        {
            return NotFound(new
            {
                message = "Appointment not found"
            });
        }

        appointment.Status = "Cancelled";
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Appointment cancelled successfully"
        });
    }
}
