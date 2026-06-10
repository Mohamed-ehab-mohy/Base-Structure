namespace Acme.SaaS.Domain.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; }
    public Guid UserId { get; }
    public decimal TotalAmount { get; }
    public DateTime OccurredAt { get; }

    public OrderCreatedEvent(Guid orderId, Guid userId, decimal totalAmount)
    {
        OrderId = orderId;
        UserId = userId;
        TotalAmount = totalAmount;
        OccurredAt = DateTime.UtcNow;
    }
}
