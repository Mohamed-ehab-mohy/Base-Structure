using MediatR;

namespace Acme.SaaS.Application.Features.Billing;

public record UpgradePlanCommand(
    Guid TenantId,
    string NewPlan
) : IRequest<Result>;
