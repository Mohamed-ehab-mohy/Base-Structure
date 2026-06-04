using Acme.SaaS.Domain.Entities;
using Acme.SaaS.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Acme.SaaS.Infrastructure.MultiTenancy.Services;

public class SchemaService
{
    private readonly MasterDbContext _masterContext;

    public SchemaService(MasterDbContext masterContext)
    {
        _masterContext = masterContext;
    }

    public async Task<string> CreateTenantSchemaAsync(Tenant tenant, CancellationToken ct = default)
    {
        var schemaName = $"tenant_{tenant.Identifier.ToLowerInvariant()}";

        await _masterContext.Database.ExecuteSqlInterpolatedAsync(
            $"CREATE SCHEMA IF NOT EXISTS [{schemaName}]", ct);

        return schemaName;
    }
}
