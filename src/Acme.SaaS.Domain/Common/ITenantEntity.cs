namespace Acme.SaaS.Domain.Common;

public interface ITenantEntity
{
    Guid TenantId { get; set; }
}
