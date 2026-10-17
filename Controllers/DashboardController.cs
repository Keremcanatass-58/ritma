using Ritma.Api.Data;
using Ritma.Api.DTOs;
using Ritma.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ritma.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DashboardController : BusinessScopedController
{
    private readonly RitmaDbContext _context;

    public DashboardController(RitmaDbContext context, ICurrentUser currentUser)
        : base(currentUser)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get()
    {
        var businessId = _currentUser.BusinessId;
        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);
        var monthStart = new DateTime(todayStart.Year, todayStart.Month, 1);

        var dashboard = await _context.Businesses
            .AsNoTracking()
            .Where(b => b.Id == businessId && b.IsActive)
            .Select(b => new DashboardDto
            {
                TodayAppointments = _context.Appointments.Count(a => a.BusinessId == businessId && a.StartTime >= todayStart && a.StartTime < tomorrowStart && a.Status != "Cancelled"),
                TotalCustomers = _context.Customers.Count(c => c.BusinessId == businessId),
                ActiveStaff = _context.Staff.Count(s => s.BusinessId == businessId && s.IsActive),
                ActiveServices = _context.Services.Count(s => s.BusinessId == businessId && s.IsActive),
                TodayRevenue = _context.Payments.Where(p => p.BusinessId == businessId && p.Status == "Paid" && p.CreatedAt >= todayStart && p.CreatedAt < tomorrowStart).Select(p => (decimal?)p.Amount).Sum() ?? 0,
                MonthlyRevenue = _context.Payments.Where(p => p.BusinessId == businessId && p.Status == "Paid" && p.CreatedAt >= monthStart && p.CreatedAt < tomorrowStart).Select(p => (decimal?)p.Amount).Sum() ?? 0,
                PendingPaymentCount = _context.Payments.Count(p => p.BusinessId == businessId && p.Status == "Pending"),
                PendingPaymentAmount = _context.Payments.Where(p => p.BusinessId == businessId && p.Status == "Pending").Select(p => (decimal?)p.Amount).Sum() ?? 0
            })
            .SingleOrDefaultAsync();

        return dashboard == null ? NotFound(new { message = "Active business not found" }) : Ok(dashboard);
    }

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports([FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null)
    {
        var businessId = _currentUser.BusinessId;
        var today = DateOnly.FromDateTime(DateTime.Today);
        var startDate = from ?? new DateOnly(today.Year, today.Month, 1);
        var endDate = to ?? today;
        if (endDate < startDate || endDate.DayNumber - startDate.DayNumber > 365)
        {
            return BadRequest(new { message = "Rapor aralığı en fazla bir yıl olabilir." });
        }

        var start = startDate.ToDateTime(TimeOnly.MinValue);
        var end = endDate.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var appointments = _context.Appointments.AsNoTracking()
            .Where(a => a.BusinessId == businessId && a.StartTime >= start && a.StartTime < end);
        var payments = _context.Payments.AsNoTracking()
            .Where(p => p.BusinessId == businessId && p.CreatedAt >= start && p.CreatedAt < end);

        var statusCounts = await appointments.GroupBy(a => a.Status)
            .Select(g => new { status = g.Key, count = g.Count() }).ToListAsync();
        var dailyAppointments = await appointments.GroupBy(a => a.StartTime.Date)
            .Select(g => new { date = g.Key, count = g.Count() }).ToListAsync();
        var dailyRevenue = await payments.Where(p => p.Status == "Paid")
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new { date = g.Key, amount = g.Sum(p => p.Amount) }).ToListAsync();
        var serviceCounts = await appointments.GroupBy(a => a.Service.Name)
            .Select(g => new { name = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count).Take(5).ToListAsync();
        var revenue = await payments.Where(p => p.Status == "Paid")
            .Select(p => (decimal?)p.Amount).SumAsync() ?? 0;
        var pending = await payments.Where(p => p.Status == "Pending")
            .Select(p => (decimal?)p.Amount).SumAsync() ?? 0;

        var daily = Enumerable.Range(0, endDate.DayNumber - startDate.DayNumber + 1)
            .Select(i => startDate.AddDays(i))
            .Select(day =>
            {
                var date = day.ToDateTime(TimeOnly.MinValue);
                return new
                {
                    date = day,
                    appointments = dailyAppointments.FirstOrDefault(x => x.date == date)?.count ?? 0,
                    revenue = dailyRevenue.FirstOrDefault(x => x.date == date)?.amount ?? 0
                };
            }).ToList();

        return Ok(new
        {
            from = startDate,
            to = endDate,
            appointmentCount = statusCounts.Sum(x => x.count),
            completedCount = statusCounts.FirstOrDefault(x => x.status == "Completed")?.count ?? 0,
            cancelledCount = statusCounts.FirstOrDefault(x => x.status == "Cancelled")?.count ?? 0,
            confirmedCount = statusCounts.FirstOrDefault(x => x.status == "Confirmed")?.count ?? 0,
            revenue,
            pendingPayments = pending,
            daily,
            topServices = serviceCounts
        });
    }
}
