using Acme.SaaS.Application.MiniServices.Tenants;
using Acme.SaaS.Application.MiniServices.Products;
using Acme.SaaS.Domain.Entities;
using AutoMapper;

namespace Acme.SaaS.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Tenant, TenantDto>().ReverseMap();
        CreateMap<Product, ProductDto>().ReverseMap();
    }
}
