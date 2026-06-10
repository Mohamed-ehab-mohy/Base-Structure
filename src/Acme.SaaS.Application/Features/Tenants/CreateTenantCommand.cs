using MediatR;

namespace Acme.SaaS.Application.Features.Tenants;

public record CreateTenantCommand(
    string Identifier,
    string? SchemaName,
    string Plan
) : IRequest<Result<Guid>>;
