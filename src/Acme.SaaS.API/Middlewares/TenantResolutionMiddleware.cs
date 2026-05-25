using Acme.SaaS.Domain.Entities;
using Acme.SaaS.Infrastructure.MultiTenancy.Store;
using Microsoft.EntityFrameworkCore;

namespace Acme.SaaS.API.Middlewares;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantStore tenantStore)
    {
        var tenantId = ResolveTenantIdentifier(context);

        if (!string.IsNullOrEmpty(tenantId))
        {
            var tenant = tenantStore.Get(tenantId) ?? await LoadTenantFromDb(context, tenantId);

            if (tenant is not null)
            {
                context.Items["TenantId"] = tenant.Id;
                context.Items["TenantIdentifier"] = tenant.Identifier;
                context.Items["SchemaName"] = tenant.SchemaName;
                context.Items["TenantPlan"] = tenant.Plan;
                context.Items["TenantStatus"] = tenant.Status;
            }
        }

        await _next(context);
    }

    private static string? ResolveTenantIdentifier(HttpContext context)
    {
        var fromHost = context.Request.Host.Host;
        var dotIndex = fromHost.IndexOf('.');
        if (dotIndex > 0)
            return fromHost[..dotIndex];

        var fromHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(fromHeader))
            return fromHeader;

        var fromClaim = context.User?.FindFirst("TenantId")?.Value;
        return fromClaim;
    }

    private static async Task<Tenant?> LoadTenantFromDb(HttpContext context, string identifier)
    {
        var dbContext = context.RequestServices.GetRequiredService<Infrastructure.Persistence.Contexts.MasterDbContext>();
        var tenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.Identifier == identifier);

        if (tenant is not null)
        {
            var store = context.RequestServices.GetRequiredService<TenantStore>();
            store.Set(tenant);
        }

        return tenant;
    }
}
