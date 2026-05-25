using Acme.SaaS.Application.Common.Interfaces;
using Acme.SaaS.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Acme.SaaS.Infrastructure.MultiTenancy.Services;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Items["TenantId"] is Guid id ? id : Guid.Empty;
    }

    public string? GetSchemaName()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Items["SchemaName"] as string;
    }

    public SubscriptionPlan GetPlan()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Items["TenantPlan"] is SubscriptionPlan plan ? plan : SubscriptionPlan.Free;
    }

    public string? GetIdentifier()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Items["TenantIdentifier"] as string;
    }
}
