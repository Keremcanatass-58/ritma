namespace Ritma.Api.Services;

public interface ICurrentUser
{
    int BusinessId { get; }
    int UserId { get; }
    string Role { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
}
