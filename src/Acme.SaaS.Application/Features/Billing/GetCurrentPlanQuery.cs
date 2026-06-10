using MediatR;

namespace Acme.SaaS.Application.Features.Billing;

public record GetCurrentPlanQuery(Guid TenantId) : IRequest<Result<PlanDto>>;

public class PlanDto
{
    public string Plan { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
}
