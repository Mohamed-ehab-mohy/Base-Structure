using System.Collections.Concurrent;
using Acme.SaaS.Domain.Entities;

namespace Acme.SaaS.Infrastructure.MultiTenancy.Store;

public class TenantStore
{
    private readonly ConcurrentDictionary<string, Tenant> _cache = new();

    public Tenant? Get(string identifier)
    {
        _cache.TryGetValue(identifier.ToLowerInvariant(), out var tenant);
        return tenant;
    }

    public void Set(Tenant tenant)
    {
        _cache[tenant.Identifier.ToLowerInvariant()] = tenant;
    }

    public void Remove(string identifier)
    {
        _cache.TryRemove(identifier.ToLowerInvariant(), out _);
    }

    public void Clear() => _cache.Clear();
}
