using Acme.SaaS.Application.MiniServices.Products;
using Microsoft.AspNetCore.Mvc;

namespace Acme.SaaS.API.Controllers;

public class ProductsController : BaseApiController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request, CancellationToken ct) =>
        ToActionResult(await _productService.CreateProductAsync(request, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        ToActionResult(await _productService.GetProductByIdAsync(id, ct));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10, CancellationToken ct = default) =>
        ToActionResult(await _productService.GetProductsListAsync(page, size, ct));
}
