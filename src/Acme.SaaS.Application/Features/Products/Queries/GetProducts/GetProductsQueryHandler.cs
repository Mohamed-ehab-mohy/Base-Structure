using MediatR;
using AutoMapper;
using Acme.SaaS.Application.Common.Models;
using Acme.SaaS.Application.Features.Products.DTOs;
using Acme.SaaS.Application.Common.Interfaces;
using Acme.SaaS.Domain.Entities;
using Acme.SaaS.Domain.Interfaces;

namespace Acme.SaaS.Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<PaginatedList<ProductDto>>>
{
    private readonly IRepository<Product> _repository;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IRepository<Product> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            products = products
                .Where(p => p.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var totalCount = products.Count;
        var items = products
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<ProductDto>>(items);
        var paginatedList = new PaginatedList<ProductDto>(dtos, totalCount, request.PageNumber, request.PageSize);

        return Result<PaginatedList<ProductDto>>.Success(paginatedList);
    }
}
