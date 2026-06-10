namespace Acme.SaaS.Domain.Exceptions;

public class ProductNotFoundException : NotFoundException
{
    public ProductNotFoundException(Guid productId)
        : base(nameof(Product), productId)
    {
    }
}
