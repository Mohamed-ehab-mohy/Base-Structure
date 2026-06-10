using MediatR;

namespace Acme.SaaS.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal Price
) : IRequest<Result>;
