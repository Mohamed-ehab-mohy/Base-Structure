using MediatR;
using Acme.SaaS.Domain.Entities;
using Acme.SaaS.Domain.Enums;
using Acme.SaaS.Domain.Interfaces;
using Acme.SaaS.Domain.Exceptions;

namespace Acme.SaaS.Application.Features.Tenants;

public class DeactivateTenantCommandHandler : IRequestHandler<DeactivateTenantCommand, Result>
{
    private readonly IRepository<Tenant> _repository;

    public DeactivateTenantCommandHandler(IRepository<Tenant> repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeactivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (tenant == null)
            throw new NotFoundException(nameof(Tenant), request.Id);

        tenant.Status = TenantStatus.Suspended;
        _repository.Update(tenant);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success("Tenant deactivated successfully");
    }
}
