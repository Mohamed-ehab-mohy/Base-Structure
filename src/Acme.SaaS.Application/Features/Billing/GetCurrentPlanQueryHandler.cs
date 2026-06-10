using MediatR;
using AutoMapper;
using Acme.SaaS.Domain.Entities;
using Acme.SaaS.Domain.Interfaces;
using Acme.SaaS.Domain.Exceptions;

namespace Acme.SaaS.Application.Features.Billing;

public class GetCurrentPlanQueryHandler : IRequestHandler<GetCurrentPlanQuery, Result<PlanDto>>
{
    private readonly IRepository<Tenant> _repository;
    private readonly IMapper _mapper;

    public GetCurrentPlanQueryHandler(IRepository<Tenant> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PlanDto>> Handle(GetCurrentPlanQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
            throw new NotFoundException(nameof(Tenant), request.TenantId);

        var dto = new PlanDto
        {
            Plan = tenant.Plan.ToString(),
            Status = tenant.Status.ToString()
        };

        return Result<PlanDto>.Success(dto);
    }
}
