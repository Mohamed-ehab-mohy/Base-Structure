using MediatR;
using Acme.SaaS.Domain.Entities;
using Acme.SaaS.Domain.Enums;
using Acme.SaaS.Domain.Interfaces;
using Acme.SaaS.Domain.Exceptions;

namespace Acme.SaaS.Application.Features.Billing;

public class UpgradePlanCommandHandler : IRequestHandler<UpgradePlanCommand, Result>
{
    private readonly IRepository<Tenant> _repository;

    public UpgradePlanCommandHandler(IRepository<Tenant> repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpgradePlanCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
            throw new NotFoundException(nameof(Tenant), request.TenantId);

        tenant.Plan = Enum.Parse<SubscriptionPlan>(request.NewPlan, ignoreCase: true);
        _repository.Update(tenant);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success($"Plan upgraded to {request.NewPlan} successfully");
    }
}
