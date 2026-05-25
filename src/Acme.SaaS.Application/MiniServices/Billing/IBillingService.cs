using Acme.SaaS.Application.Common.DTOs;

namespace Acme.SaaS.Application.MiniServices.Billing;

public interface IBillingService
{
    Task<ApiResponse<string>> GetCurrentPlanAsync(CancellationToken ct);
    Task<ApiResponse<bool>> UpgradePlanAsync(string plan, CancellationToken ct);
}

public record BillingPlanDto(string Plan, decimal Price);
