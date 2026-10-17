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
public class StaffController : BusinessScopedController
{
    private readonly RitmaDbContext _context;

    public StaffController(RitmaDbContext context, ICurrentUser currentUser)
        : base(currentUser)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Create(CreateStaffDto dto)
    {
        var businessId = GetBusinessId();

        var staff = new Staff
        {
            BusinessId = businessId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone,
            Email = dto.Email,
            Role = dto.Role
        };

        _context.Staff.Add(staff);
        await _context.SaveChangesAsync();

        return Ok(staff);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20)
    {
        var businessId = GetBusinessId();

        var query = _context.Staff
            .AsNoTracking()
            .Where(s => s.BusinessId == businessId);

        if (!includeInactive)
        {
            query = query.Where(s => s.IsActive);
        }

        query = query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName);

        var totalItems = await query.CountAsync();
        var staff = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { items = staff, totalItems, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var businessId = GetBusinessId();

        var staff = await _context.Staff
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == id &&
                s.BusinessId == businessId);

        if (staff == null)
        {
            return NotFound(new
            {
                message = "Staff not found for this business"
            });
        }

        return Ok(staff);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Update(
        int id,
        UpdateStaffDto dto)
    {
        var businessId = GetBusinessId();

        var staff = await _context.Staff
            .FirstOrDefaultAsync(s =>
                s.Id == id &&
                s.BusinessId == businessId);

        if (staff == null)
        {
            return NotFound(new
            {
                message = "Staff not found for this business"
            });
        }

        staff.FirstName = dto.FirstName;
        staff.LastName = dto.LastName;
        staff.Phone = dto.Phone;
        staff.Email = dto.Email;
        staff.Role = dto.Role;
        staff.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(staff);
    }

    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var businessId = GetBusinessId();

        var staff = await _context.Staff
            .FirstOrDefaultAsync(s =>
                s.Id == id &&
                s.BusinessId == businessId);

        if (staff == null)
        {
            return NotFound(new
            {
                message = "Staff not found for this business"
            });
        }

        staff.IsActive = false;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Staff deactivated successfully."
        });
    }
}
