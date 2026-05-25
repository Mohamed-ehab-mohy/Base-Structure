namespace Acme.SaaS.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
