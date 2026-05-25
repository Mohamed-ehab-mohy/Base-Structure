using Acme.SaaS.Application.Common.DTOs;
using Acme.SaaS.Application.Common.Interfaces;
using Acme.SaaS.Domain.Entities;
using Acme.SaaS.Domain.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Acme.SaaS.Application.MiniServices.Tenants;

public class TenantService : ITenantService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public TenantService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<Guid>> CreateTenantAsync(CreateTenantRequest request, CancellationToken ct)
    {
        var tenant = new Tenant
        {
            Identifier = request.Identifier,
            SchemaName = $"tenant_{request.Identifier.ToLowerInvariant()}",
            Plan = Enum.TryParse<SubscriptionPlan>(request.Plan, true, out var plan) ? plan : SubscriptionPlan.Free,
            Status = TenantStatus.Trial
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(ct);

        return ApiResponse<Guid>.Ok(tenant.Id, "Tenant created successfully.");
    }

    public async Task<ApiResponse<TenantDto>> GetTenantByIdAsync(Guid tenantId, CancellationToken ct)
    {
        var tenant = await _context.Tenants.FindAsync([tenantId], ct);
        if (tenant is null)
            return ApiResponse<TenantDto>.Fail("Tenant not found.");

        var dto = _mapper.Map<TenantDto>(tenant);
        return ApiResponse<TenantDto>.Ok(dto);
    }

    public async Task<ApiResponse<PagedResult<TenantDto>>> GetTenantsListAsync(int page, int size, CancellationToken ct)
    {
        var query = _context.Tenants.OrderBy(t => t.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);
        var dtos = _mapper.Map<List<TenantDto>>(items);

        return ApiResponse<PagedResult<TenantDto>>.Ok(new PagedResult<TenantDto>
        {
            Items = dtos, TotalCount = total, Page = page, Size = size
        });
    }

    public async Task<ApiResponse<bool>> DeactivateTenantAsync(Guid tenantId, CancellationToken ct)
    {
        var tenant = await _context.Tenants.FindAsync([tenantId], ct);
        if (tenant is null)
            return ApiResponse<bool>.Fail("Tenant not found.");

        tenant.Status = TenantStatus.Suspended;
        await _context.SaveChangesAsync(ct);

        return ApiResponse<bool>.Ok(true, "Tenant deactivated.");
    }
}
