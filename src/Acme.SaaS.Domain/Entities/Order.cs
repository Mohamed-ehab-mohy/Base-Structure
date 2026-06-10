using Acme.SaaS.Domain.Enums;

namespace Acme.SaaS.Domain.Entities;

public class Order : BaseAuditableEntity
{
    public Guid UserId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime OrderDate { get; private set; }

    private Order() { }

    public Order(Guid userId, decimal totalAmount)
    {
        UserId = userId;
        TotalAmount = totalAmount;
        Status = OrderStatus.Pending;
        OrderDate = DateTime.UtcNow;
    }

    public void MarkAsProcessing()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Order must be in Pending status to start processing");
        Status = OrderStatus.Processing;
    }

    public void MarkAsShipped()
    {
        if (Status != OrderStatus.Processing)
            throw new DomainException("Order must be in Processing status to ship");
        Status = OrderStatus.Shipped;
    }

    public void MarkAsDelivered()
    {
        if (Status != OrderStatus.Shipped)
            throw new DomainException("Order must be in Shipped status to deliver");
        Status = OrderStatus.Delivered;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
            throw new DomainException("Cannot cancel a delivered order");
        if (Status == OrderStatus.Cancelled)
            throw new DomainException("Order is already cancelled");
        Status = OrderStatus.Cancelled;
    }
}
