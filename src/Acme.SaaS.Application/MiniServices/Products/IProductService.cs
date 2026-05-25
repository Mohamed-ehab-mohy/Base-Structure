using Acme.SaaS.Application.Common.DTOs;

namespace Acme.SaaS.Application.MiniServices.Products;

public interface IProductService
{
    Task<ApiResponse<Guid>> CreateProductAsync(CreateProductRequest request, CancellationToken ct);
    Task<ApiResponse<ProductDto>> GetProductByIdAsync(Guid id, CancellationToken ct);
    Task<ApiResponse<PagedResult<ProductDto>>> GetProductsListAsync(int page, int size, CancellationToken ct);
}

public record CreateProductRequest(string Name, string? Description, decimal Price);
public record ProductDto(Guid Id, string Name, string? Description, decimal Price);
