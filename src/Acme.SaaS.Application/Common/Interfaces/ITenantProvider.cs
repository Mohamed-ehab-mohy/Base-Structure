using Acme.SaaS.Domain.Enums;

namespace Acme.SaaS.Application.Common.Interfaces;

public interface ITenantProvider
{
    Guid GetTenantId();
    string? GetSchemaName();
    SubscriptionPlan GetPlan();
    string? GetIdentifier();
}
