namespace Ritma.Api.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public int UserId { get; set; }

    public int BusinessId { get; set; }

    public string Role { get; set; } = string.Empty;
}
