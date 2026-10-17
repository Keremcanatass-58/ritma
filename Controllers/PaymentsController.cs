using Ritma.Api.Data;
using Ritma.Api.DTOs;
using Ritma.Api.Models;
using Ritma.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Ritma.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PaymentsController : BusinessScopedController
{
    private readonly RitmaDbContext _context;

    public PaymentsController(RitmaDbContext context, ICurrentUser currentUser)
        : base(currentUser)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Create(CreatePaymentDto dto)
    {
        var businessId = GetBusinessId();

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a =>
                a.Id == dto.AppointmentId &&
                a.BusinessId == businessId &&
                a.CustomerId == dto.CustomerId);

        if (appointment == null)
        {
            return NotFound(new { message = "Appointment not found for this business and customer" });
        }

        var alreadyPaid = await _context.Payments
            .AnyAsync(p => p.AppointmentId == dto.AppointmentId);

        if (alreadyPaid)
        {
            return Conflict(new { message = "A payment already exists for this appointment" });
        }

        var payment = new Payment
        {
            BusinessId = businessId,
            CustomerId = dto.CustomerId,
            AppointmentId = dto.AppointmentId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            Status = dto.Status,
            Notes = dto.Notes
        };

        _context.Payments.Add(payment);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "A payment already exists for this appointment" });
        }

        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20)
    {
        var businessId = GetBusinessId();

        var query = _context.Payments
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Appointment)
            .Where(p => p.BusinessId == businessId)
            .OrderByDescending(p => p.CreatedAt);

        var totalItems = await query.CountAsync();
        var payments = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { items = payments, totalItems, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var businessId = GetBusinessId();

        var payment = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Appointment)
            .FirstOrDefaultAsync(p => p.Id == id && p.BusinessId == businessId);

        if (payment == null)
        {
            return NotFound(new { message = "Payment not found for this business" });
        }

        return Ok(payment);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Update(int id, UpdatePaymentDto dto)
    {
        var businessId = GetBusinessId();

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id && p.BusinessId == businessId);

        if (payment == null)
        {
            return NotFound(new { message = "Payment not found for this business" });
        }

        // Amount cannot be changed after creation — only Status and Notes
        payment.Status = dto.Status;
        payment.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return Ok(payment);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        return StatusCode(StatusCodes.Status405MethodNotAllowed, new
        {
            message = "Payments cannot be deleted. Use a status update to cancel or refund a payment."
        });
    }
}
