using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ritma.Api.Data;
using Ritma.Api.DTOs;
using Ritma.Api.Models;
using Ritma.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Ritma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RitmaDbContext _context;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUser _currentUser;

    public AuthController(RitmaDbContext context, IPasswordHasher<AppUser> passwordHasher, IConfiguration configuration, ICurrentUser currentUser)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _currentUser = currentUser;
    }

    [AllowAnonymous]
    [EnableRateLimiting("owner-registration")]
    [HttpPost("register-owner")]
    public async Task<IActionResult> RegisterOwner(RegisterOwnerDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            return Conflict(new { message = "An account already exists for this email" });
        }

        var business = new Business
        {
            Name = dto.BusinessName.Trim(),
            Phone = dto.BusinessPhone,
            Email = dto.BusinessEmail,
            Address = dto.BusinessAddress
        };
        var user = new AppUser
        {
            Business = business,
            FullName = dto.FullName.Trim(),
            Email = email,
            Role = "Owner"
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, CreateAuthResponse(user));
    }

    [Authorize(Roles = "Owner")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            return Conflict(new { message = "An account already exists for this email" });
        }

        var user = new AppUser
        {
            BusinessId = _currentUser.BusinessId,
            FullName = dto.FullName.Trim(),
            Email = email,
            Role = dto.Role
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new { user.Id, user.FullName, user.Email, user.Role });
    }

    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.Include(u => u.Business).FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !user.IsActive || !user.Business.IsActive || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        return Ok(CreateAuthResponse(user));
    }

    private AuthResponseDto CreateAuthResponse(AppUser user)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key is not configured.");
        var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 480);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("businessId", user.BusinessId.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Email, user.Email)
        };
        var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], claims, expires: expiresAt, signingCredentials: credentials);

        return new AuthResponseDto { Token = new JwtSecurityTokenHandler().WriteToken(token), ExpiresAt = expiresAt, UserId = user.Id, BusinessId = user.BusinessId, Role = user.Role };
    }
}
