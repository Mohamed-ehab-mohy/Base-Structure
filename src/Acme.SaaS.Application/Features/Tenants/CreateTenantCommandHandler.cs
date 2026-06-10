using MediatR;
using Acme.SaaS.Domain.Entities;
using Acme.SaaS.Domain.Enums;
using Acme.SaaS.Domain.Interfaces;

namespace Acme.SaaS.Application.Features.Tenants;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<Guid>>
{
    private readonly IRepository<Tenant> _repository;

    public CreateTenantCommandHandler(IRepository<Tenant> repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var plan = Enum.Parse<SubscriptionPlan>(request.Plan, ignoreCase: true);

        var tenant = new Tenant
        {
            Identifier = request.Identifier,
            SchemaName = request.SchemaName ?? $"tenant_{request.Identifier.ToLowerInvariant()}",
            Plan = plan,
            Status = TenantStatus.Active
        };

        _repository.Add(tenant);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(tenant.Id, "Tenant created successfully");
    }
}
