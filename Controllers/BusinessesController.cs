using Ritma.Api.Data;
using Ritma.Api.DTOs;
using Ritma.Api.Models;
using Ritma.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ritma.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BusinessesController : BusinessScopedController
{
    private readonly RitmaDbContext _context;

    public BusinessesController(RitmaDbContext context, ICurrentUser currentUser)
        : base(currentUser)
    {
        _context = context;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrent()
    {
        var businessId = GetBusinessId();
        var business = await _context.Businesses.FindAsync(businessId);

        if (business == null)
        {
            return NotFound(new { message = "Business not found" });
        }

        return Ok(business);
    }

    [HttpPut("me")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> UpdateCurrent(UpdateBusinessSettingsDto dto)
    {
        if (dto.ClosingTime <= dto.OpeningTime)
        {
            return BadRequest(new { message = "Kapanış saati açılış saatinden sonra olmalı." });
        }

        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == GetBusinessId() && b.IsActive);

        if (business == null)
        {
            return NotFound(new { message = "İşletme bulunamadı." });
        }

        business.Name = dto.Name.Trim();
        business.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        business.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        business.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
        business.OpeningTime = dto.OpeningTime;
        business.ClosingTime = dto.ClosingTime;
        business.WorkingDaysMask = dto.WorkingDaysMask;
        business.BookingEnabled = dto.BookingEnabled;

        await _context.SaveChangesAsync();
        return Ok(business);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var businessId = GetBusinessId();
        if (id != businessId)
        {
            return Forbid();
        }

        var business = await _context.Businesses.FindAsync(businessId);

        if (business == null)
        {
            return NotFound(new
            {
                message = "Business not found"
            });
        }

        return Ok(business);
    }
}
