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
public class CustomersController : BusinessScopedController
{
    private readonly RitmaDbContext _context;

    public CustomersController(RitmaDbContext context, ICurrentUser currentUser)
        : base(currentUser)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerDto dto)
    {
        var businessId = GetBusinessId();

        var customer = new Customer
        {
            BusinessId = businessId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone,
            Email = dto.Email,
            Notes = dto.Notes
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return Ok(customer);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20)
    {
        var businessId = GetBusinessId();

        var query = _context.Customers
            .AsNoTracking()
            .Where(c => c.BusinessId == businessId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(s) ||
                c.LastName.ToLower().Contains(s) ||
                (c.Phone != null && c.Phone.Contains(s)) ||
                (c.Email != null && c.Email.ToLower().Contains(s)));
        }

        query = query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName);

        var totalItems = await query.CountAsync();
        var customers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { items = customers, totalItems, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var businessId = GetBusinessId();

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                c.BusinessId == businessId);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found for this business"
            });
        }

        return Ok(customer);
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var businessId = GetBusinessId();
        var customer = await _context.Customers.AsNoTracking()
            .Where(c => c.Id == id && c.BusinessId == businessId)
            .Select(c => new { c.Id, c.FirstName, c.LastName, c.Phone, c.Email, c.Notes, c.CreatedAt })
            .FirstOrDefaultAsync();

        if (customer == null)
        {
            return NotFound(new { message = "Müşteri bulunamadı." });
        }

        var appointments = await _context.Appointments.AsNoTracking()
            .Where(a => a.BusinessId == businessId && a.CustomerId == id)
            .OrderByDescending(a => a.StartTime)
            .Select(a => new
            {
                a.Id, a.StartTime, a.EndTime, a.Status,
                service = a.Service.Name,
                staff = a.Staff.FirstName + " " + a.Staff.LastName,
                payment = _context.Payments.Where(p => p.AppointmentId == a.Id)
                    .Select(p => new { p.Amount, p.Status, p.PaymentMethod }).FirstOrDefault()
            })
            .Take(100)
            .ToListAsync();

        return Ok(new { customer, appointments });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCustomerDto dto)
    {
        var businessId = GetBusinessId();

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id && c.BusinessId == businessId);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found"
            });
        }

        customer.FirstName = dto.FirstName;
        customer.LastName = dto.LastName;
        customer.Phone = dto.Phone;
        customer.Email = dto.Email;
        customer.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return Ok(customer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var businessId = GetBusinessId();

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id && c.BusinessId == businessId);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found"
            });
        }

        var hasRelatedRecords = await _context.Appointments
            .AnyAsync(a => a.CustomerId == customer.Id) ||
            await _context.Payments.AnyAsync(p => p.CustomerId == customer.Id);

        if (hasRelatedRecords)
        {
            return Conflict(new
            {
                message = "A customer with appointment or payment history cannot be deleted."
            });
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Customer deleted successfully"
        });
    }
}
