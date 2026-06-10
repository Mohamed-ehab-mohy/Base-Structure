using MediatR;
using AutoMapper;
using Acme.SaaS.Application.Common.Interfaces;
using Acme.SaaS.Domain.Entities;
using Acme.SaaS.Domain.Interfaces;

namespace Acme.SaaS.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IRepository<Product> _repository;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(IRepository<Product> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price
        };

        _repository.Add(product);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(product.Id, "Product created successfully");
    }
}
