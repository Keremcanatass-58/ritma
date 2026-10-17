using System.Security.Claims;

namespace Ritma.Api.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int BusinessId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue("businessId");
            return int.TryParse(value, out var businessId) ? businessId : 0;
        }
    }

    public int UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : 0;
        }
    }

    public string Role => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    public string Email => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
