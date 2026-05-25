using Acme.SaaS.Application.MiniServices.Billing;
using Acme.SaaS.Application.MiniServices.Identity;
using Acme.SaaS.Application.MiniServices.Products;
using Acme.SaaS.Application.MiniServices.Tenants;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Acme.SaaS.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Mapping.MappingProfile));

        services.AddValidatorsFromAssemblyContaining<Mapping.MappingProfile>();

        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IBillingService, BillingService>();

        return services;
    }
}
