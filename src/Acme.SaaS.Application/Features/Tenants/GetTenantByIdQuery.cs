using MediatR;

namespace Acme.SaaS.Application.Features.Tenants;

public record GetTenantByIdQuery(Guid Id) : IRequest<Result<TenantDto>>;

public class TenantDto
{
    public Guid Id { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string? SchemaName { get; set; }
    public string Plan { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
