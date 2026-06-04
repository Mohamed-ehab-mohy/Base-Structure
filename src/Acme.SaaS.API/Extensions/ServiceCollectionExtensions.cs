using System.Text;
using Acme.SaaS.Infrastructure.Extensions;
using Acme.SaaS.Infrastructure.MultiTenancy;
using Acme.SaaS.Infrastructure.Services.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace Acme.SaaS.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwt = configuration.GetSection("Jwt");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt["Issuer"],
                    ValidAudience = jwt["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt["Secret"] ?? "DefaultSecretKey-ChangeMe-InProduction!"))
                };
            });

        services.AddAuthorization();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Acme SaaS API",
                Version = "v1",
                Description = "Multi-tenant SaaS API with Clean Architecture"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT token"
            });
        });

        services.AddControllers();

        return services;
    }

    public static IServiceCollection AddTenantDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MasterDb");
        var tenancyOptions = configuration.GetSection("TenancyOptions").Get<TenancyOptions>() ?? new TenancyOptions();
        services.AddSingleton(tenancyOptions);
        services.AddInfrastructureLayer(connectionString!);

        return services;
    }
}
