namespace Acme.SaaS.Domain.Events;

public class UserRegisteredEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public DateTime OccurredAt { get; }

    public UserRegisteredEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
        OccurredAt = DateTime.UtcNow;
    }
}
