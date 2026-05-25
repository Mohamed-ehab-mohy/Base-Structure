using Acme.SaaS.Application.Common.DTOs;

namespace Acme.SaaS.Application.MiniServices.Tenants;

public interface ITenantService
{
    Task<ApiResponse<Guid>> CreateTenantAsync(CreateTenantRequest request, CancellationToken ct);
    Task<ApiResponse<TenantDto>> GetTenantByIdAsync(Guid tenantId, CancellationToken ct);
    Task<ApiResponse<PagedResult<TenantDto>>> GetTenantsListAsync(int page, int size, CancellationToken ct);
    Task<ApiResponse<bool>> DeactivateTenantAsync(Guid tenantId, CancellationToken ct);
}

public record CreateTenantRequest(string Identifier, string Plan);
public record TenantDto(Guid Id, string Identifier, string Plan, string Status);
