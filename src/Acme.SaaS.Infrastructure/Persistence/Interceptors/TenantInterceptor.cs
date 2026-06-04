using Acme.SaaS.Application.Common.Interfaces;
using Acme.SaaS.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Acme.SaaS.Infrastructure.Persistence.Interceptors;

public class TenantInterceptor : SaveChangesInterceptor
{
    private readonly ITenantProvider _tenantProvider;

    public TenantInterceptor(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        if (eventData.Context is null)
            return base.SavingChangesAsync(eventData, result, ct);

        var tenantId = _tenantProvider.GetTenantId();

        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry.Entity is ITenantEntity tenantEntity && entry.State == EntityState.Added)
                tenantEntity.TenantId = tenantId;
        }

        return base.SavingChangesAsync(eventData, result, ct);
    }
}
