using Acme.SaaS.Application.Common.DTOs;
using Acme.SaaS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Acme.SaaS.Application.MiniServices.Billing;

public class BillingService : IBillingService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public BillingService(IApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<ApiResponse<string>> GetCurrentPlanAsync(CancellationToken ct)
    {
        var plan = _tenantProvider.GetPlan().ToString();
        return ApiResponse<string>.Ok(plan);
    }

    public async Task<ApiResponse<bool>> UpgradePlanAsync(UpgradePlanRequest request, CancellationToken ct)
    {
        var tenant = await _context.Tenants.FindAsync([_tenantProvider.GetTenantId()], ct);
        if (tenant is null)
            return ApiResponse<bool>.Fail("Tenant not found.");

        if (Enum.TryParse<Domain.Enums.SubscriptionPlan>(request.Plan, true, out var parsed))
            tenant.Plan = parsed;

        await _context.SaveChangesAsync(ct);
        return ApiResponse<bool>.Ok(true, $"Plan upgraded to {request.Plan}.");
    }
}
