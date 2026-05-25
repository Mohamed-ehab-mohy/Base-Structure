using Acme.SaaS.Application.Common.DTOs;
using Acme.SaaS.Application.Common.Interfaces;
using Acme.SaaS.Domain.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Acme.SaaS.Application.MiniServices.Products;

public class ProductService : IProductService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITenantProvider _tenantProvider;

    public ProductService(IApplicationDbContext context, IMapper mapper, ITenantProvider tenantProvider)
    {
        _context = context;
        _mapper = mapper;
        _tenantProvider = tenantProvider;
    }

    public async Task<ApiResponse<Guid>> CreateProductAsync(CreateProductRequest request, CancellationToken ct)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            TenantId = _tenantProvider.GetTenantId()
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);

        return ApiResponse<Guid>.Ok(product.Id, "Product created.");
    }

    public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(Guid id, CancellationToken ct)
    {
        var product = await _context.Products.FindAsync([id], ct);
        if (product is null || product.TenantId != _tenantProvider.GetTenantId())
            return ApiResponse<ProductDto>.Fail("Product not found.");

        return ApiResponse<ProductDto>.Ok(_mapper.Map<ProductDto>(product));
    }

    public async Task<ApiResponse<PagedResult<ProductDto>>> GetProductsListAsync(int page, int size, CancellationToken ct)
    {
        var query = _context.Products
            .Where(p => p.TenantId == _tenantProvider.GetTenantId())
            .OrderByDescending(p => p.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        return ApiResponse<PagedResult<ProductDto>>.Ok(new PagedResult<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(items),
            TotalCount = total,
            Page = page,
            Size = size
        });
    }
}
