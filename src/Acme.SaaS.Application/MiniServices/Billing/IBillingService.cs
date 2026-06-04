using Acme.SaaS.Application.Common.DTOs;

namespace Acme.SaaS.Application.MiniServices.Billing;

public interface IBillingService
{
    Task<ApiResponse<string>> GetCurrentPlanAsync(CancellationToken ct);
    Task<ApiResponse<bool>> UpgradePlanAsync(UpgradePlanRequest request, CancellationToken ct);
}

public record UpgradePlanRequest(string Plan);
public record BillingPlanDto(string Plan, decimal Price);
