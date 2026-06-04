using Acme.SaaS.Application.Common.Interfaces;
using Acme.SaaS.Infrastructure.MultiTenancy.Services;
using Acme.SaaS.Infrastructure.MultiTenancy.Store;
using Acme.SaaS.Infrastructure.Persistence.Contexts;
using Acme.SaaS.Infrastructure.Persistence.Interceptors;
using Acme.SaaS.Infrastructure.Persistence.Repositories;
using Acme.SaaS.Infrastructure.Services.FeatureGating;
using Acme.SaaS.Infrastructure.Services.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Acme.SaaS.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayer(
        this IServiceCollection services, string masterConnectionString)
    {
        services.AddDbContext<MasterDbContext>(options =>
            options.UseSqlServer(masterConnectionString));

        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<TenantInterceptor>();

        services.AddSingleton<TenantStore>();
        services.AddScoped<SchemaService>();
        services.AddScoped(_ => new TenantConnectionService(masterConnectionString));

        services.AddScoped<IFeatureGatingService, FeatureGatingService>();
        services.AddScoped<JwtTokenService>();

        return services;
    }
}
