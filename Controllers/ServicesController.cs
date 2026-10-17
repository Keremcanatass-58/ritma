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
public class ServicesController : BusinessScopedController
{
    private readonly RitmaDbContext _context;

    public ServicesController(RitmaDbContext context, ICurrentUser currentUser)
        : base(currentUser)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Create(CreateServiceDto dto)
    {
        var businessId = GetBusinessId();

        var service = new Service
        {
            BusinessId = businessId,
            Name = dto.Name,
            Description = dto.Description,
            DurationMinutes = dto.DurationMinutes,
            Price = dto.Price
        };

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        return Ok(service);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20)
    {
        var businessId = GetBusinessId();

        var query = _context.Services
            .AsNoTracking()
            .Where(s => s.BusinessId == businessId);

        if (!includeInactive)
        {
            query = query.Where(s => s.IsActive);
        }

        query = query.OrderBy(s => s.Name);

        var totalItems = await query.CountAsync();
        var services = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { items = services, totalItems, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var businessId = GetBusinessId();
        var service = await _context.Services.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.BusinessId == businessId);
        return service == null
            ? NotFound(new { message = "Hizmet bulunamadı." })
            : Ok(service);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Update(int id, UpdateServiceDto dto)
    {
        var businessId = GetBusinessId();

        var service = await _context.Services
            .FirstOrDefaultAsync(s =>
                s.Id == id &&
                s.BusinessId == businessId);

        if (service == null)
        {
            return NotFound(new
            {
                message = "Service not found for this business"
            });
        }

        service.Name = dto.Name;
        service.Description = dto.Description;
        service.DurationMinutes = dto.DurationMinutes;
        service.Price = dto.Price;
        service.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(service);
    }

    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var businessId = GetBusinessId();

        var service = await _context.Services
            .FirstOrDefaultAsync(s =>
                s.Id == id &&
                s.BusinessId == businessId);

        if (service == null)
        {
            return NotFound(new
            {
                message = "Service not found for this business"
            });
        }

        service.IsActive = false;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Service deactivated successfully."
        });
    }
}
