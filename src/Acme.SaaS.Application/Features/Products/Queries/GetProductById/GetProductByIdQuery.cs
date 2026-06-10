using MediatR;
using Acme.SaaS.Application.Features.Products.DTOs;

namespace Acme.SaaS.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;
