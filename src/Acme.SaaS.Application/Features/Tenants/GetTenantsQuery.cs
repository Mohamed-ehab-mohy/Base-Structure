using MediatR;
using Acme.SaaS.Application.Common.Models;

namespace Acme.SaaS.Application.Features.Tenants;

public record GetTenantsQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<PaginatedList<TenantDto>>>;
