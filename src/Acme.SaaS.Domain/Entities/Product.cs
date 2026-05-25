namespace Acme.SaaS.Domain.Entities;

public class Product : Common.BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public Guid TenantId { get; set; }
}
