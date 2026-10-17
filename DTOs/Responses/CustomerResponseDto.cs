namespace Ritma.Api.DTOs.Responses;

public class CustomerResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
