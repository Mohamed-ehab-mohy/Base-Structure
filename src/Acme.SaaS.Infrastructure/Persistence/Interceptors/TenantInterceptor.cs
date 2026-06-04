using Acme.SaaS.Application.Common.Interfaces;
using Acme.SaaS.Domain.Common;
using Acme.SaaS.Domain.Exceptions;
using Acme.SaaS.Infrastructure.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Acme.SaaS.Infrastructure.Persistence.Interceptors;

public class TenantInterceptor : SaveChangesInterceptor
{
    private readonly ITenantProvider _tenantProvider;
    private readonly TenancyOptions _tenancyOptions;

    public TenantInterceptor(ITenantProvider tenantProvider, TenancyOptions tenancyOptions)
    {
        _tenantProvider = tenantProvider;
        _tenancyOptions = tenancyOptions;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        if (eventData.Context is null)
            return base.SavingChangesAsync(eventData, result, ct);

        var tenantId = _tenantProvider.GetTenantId();

        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry.Entity is ITenantEntity tenantEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    tenantEntity.TenantId = tenantId;
                }
                else if (entry.State == EntityState.Modified && _tenancyOptions.Mode == TenancyMode.SharedSchema)
                {
                    var originalTenantId = entry.OriginalValues.GetValue<Guid>(nameof(ITenantEntity.TenantId));
                    if (originalTenantId != Guid.Empty && originalTenantId != tenantEntity.TenantId)
                        throw new DomainException("Cannot transfer data to another tenant.");
                }
            }
        }

        return base.SavingChangesAsync(eventData, result, ct);
    }
}
