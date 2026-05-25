using Acme.SaaS.Application.MiniServices.Tenants;
using Microsoft.AspNetCore.Mvc;

namespace Acme.SaaS.API.Controllers;

public class TenantsController : BaseApiController
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTenantRequest request, CancellationToken ct) =>
        ToActionResult(await _tenantService.CreateTenantAsync(request, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        ToActionResult(await _tenantService.GetTenantByIdAsync(id, ct));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10, CancellationToken ct = default) =>
        ToActionResult(await _tenantService.GetTenantsListAsync(page, size, ct));

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct) =>
        ToActionResult(await _tenantService.DeactivateTenantAsync(id, ct));
}
