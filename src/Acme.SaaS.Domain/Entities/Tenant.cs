using Acme.SaaS.Domain.Enums;

namespace Acme.SaaS.Domain.Entities;

public class Tenant : Common.BaseAuditableEntity
{
    public string Identifier { get; set; } = string.Empty;
    public string? SchemaName { get; set; }
    public string? ConnectionString { get; set; }
    public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;
    public TenantStatus Status { get; set; } = TenantStatus.Trial;
}
