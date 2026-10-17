using Ritma.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace Ritma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly RitmaDbContext _context;

    public HealthController(RitmaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var dbConnected = await _context.Database.CanConnectAsync();

        var result = new
        {
            status = dbConnected ? "healthy" : "unhealthy",
            database = dbConnected ? "connected" : "disconnected",
            service = "RITMA API",
            version = "1.0.0"
        };

        return dbConnected ? Ok(result) : StatusCode(StatusCodes.Status503ServiceUnavailable, result);
    }
}
