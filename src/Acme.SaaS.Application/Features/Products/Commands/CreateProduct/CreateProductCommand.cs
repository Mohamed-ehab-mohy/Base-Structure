using MediatR;

namespace Acme.SaaS.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price
) : IRequest<Result<Guid>>;
