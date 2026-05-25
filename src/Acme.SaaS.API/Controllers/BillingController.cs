using Acme.SaaS.Application.MiniServices.Billing;
using Microsoft.AspNetCore.Mvc;

namespace Acme.SaaS.API.Controllers;

public class BillingController : BaseApiController
{
    private readonly IBillingService _billingService;

    public BillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpGet("plan")]
    public async Task<IActionResult> GetPlan(CancellationToken ct) =>
        ToActionResult(await _billingService.GetCurrentPlanAsync(ct));

    [HttpPost("upgrade")]
    public async Task<IActionResult> Upgrade([FromBody] string plan, CancellationToken ct) =>
        ToActionResult(await _billingService.UpgradePlanAsync(plan, ct));
}
