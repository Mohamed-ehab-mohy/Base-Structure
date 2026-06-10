using MediatR;
using AutoMapper;
using Acme.SaaS.Application.Common.Interfaces;
using Acme.SaaS.Domain.Entities;
using Acme.SaaS.Domain.Interfaces;
using Acme.SaaS.Domain.Exceptions;

namespace Acme.SaaS.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IRepository<Product> _repository;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(IRepository<Product> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
            throw new NotFoundException(nameof(Product), request.Id);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;

        _repository.Update(product);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success("Product updated successfully");
    }
}
