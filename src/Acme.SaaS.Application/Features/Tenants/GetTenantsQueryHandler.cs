using MediatR;
using AutoMapper;
using Acme.SaaS.Application.Common.Models;
using Acme.SaaS.Domain.Entities;
using Acme.SaaS.Domain.Interfaces;

namespace Acme.SaaS.Application.Features.Tenants;

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, Result<PaginatedList<TenantDto>>>
{
    private readonly IRepository<Tenant> _repository;
    private readonly IMapper _mapper;

    public GetTenantsQueryHandler(IRepository<Tenant> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<TenantDto>>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _repository.GetAllAsync(cancellationToken);
        var totalCount = tenants.Count;
        var items = tenants
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<TenantDto>>(items);
        var paginatedList = new PaginatedList<TenantDto>(dtos, totalCount, request.PageNumber, request.PageSize);

        return Result<PaginatedList<TenantDto>>.Success(paginatedList);
    }
}
