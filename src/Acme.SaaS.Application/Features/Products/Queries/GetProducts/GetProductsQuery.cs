using MediatR;
using Acme.SaaS.Application.Common.Models;
using Acme.SaaS.Application.Features.Products.DTOs;

namespace Acme.SaaS.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null
) : IRequest<Result<PaginatedList<ProductDto>>>;
