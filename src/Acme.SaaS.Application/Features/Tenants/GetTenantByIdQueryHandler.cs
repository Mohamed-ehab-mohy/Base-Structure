using MediatR;
using AutoMapper;
using Acme.SaaS.Domain.Entities;
using Acme.SaaS.Domain.Interfaces;
using Acme.SaaS.Domain.Exceptions;

namespace Acme.SaaS.Application.Features.Tenants;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, Result<TenantDto>>
{
    private readonly IRepository<Tenant> _repository;
    private readonly IMapper _mapper;

    public GetTenantByIdQueryHandler(IRepository<Tenant> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<TenantDto>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (tenant == null)
            throw new NotFoundException(nameof(Tenant), request.Id);

        var dto = _mapper.Map<TenantDto>(tenant);
        return Result<TenantDto>.Success(dto);
    }
}
