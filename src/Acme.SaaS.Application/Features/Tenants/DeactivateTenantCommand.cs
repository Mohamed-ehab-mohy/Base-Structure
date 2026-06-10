using MediatR;

namespace Acme.SaaS.Application.Features.Tenants;

public record DeactivateTenantCommand(Guid Id) : IRequest<Result>;
