using System.Linq.Expressions;
using Acme.SaaS.Application.Common.Interfaces;
using Acme.SaaS.Domain.Common;
using Acme.SaaS.Infrastructure.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Acme.SaaS.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly DbContext Context;
    protected readonly DbSet<T> Set;
    protected readonly ITenantProvider TenantProvider;
    protected readonly TenancyOptions TenancyOptions;

    public GenericRepository(IApplicationDbContext context, ITenantProvider tenantProvider, TenancyOptions tenancyOptions)
    {
        Context = (DbContext)context;
        Set = Context.Set<T>();
        TenantProvider = tenantProvider;
        TenancyOptions = tenancyOptions;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await Set.FindAsync([id], ct);

        if (entity is ITenantEntity tenantEntity && TenancyOptions.Mode == TenancyMode.SharedSchema
            && tenantEntity.TenantId != TenantProvider.GetTenantId())
            return null;

        return entity;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        var query = Set.AsQueryable();
        var filter = BuildTenantFilter();
        if (filter is not null)
            query = query.Where(filter);
        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        var query = Set.Where(predicate);
        var filter = BuildTenantFilter();
        if (filter is not null)
            query = query.Where(filter);
        return await query.ToListAsync(ct);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        var query = Set.Where(predicate);
        var filter = BuildTenantFilter();
        if (filter is not null)
            query = query.Where(filter);
        return await query.AnyAsync(ct);
    }

    public void Add(T entity) => Set.Add(entity);

    public void Update(T entity) => Set.Update(entity);

    public void Delete(T entity) => Set.Remove(entity);

    private Expression<Func<T, bool>>? BuildTenantFilter()
    {
        if (TenancyOptions.Mode != TenancyMode.SharedSchema)
            return null;

        if (!typeof(ITenantEntity).IsAssignableFrom(typeof(T)))
            return null;

        var param = Expression.Parameter(typeof(T), "e");
        var tenantIdProp = Expression.Property(param, nameof(ITenantEntity.TenantId));
        var tenantIdValue = Expression.Constant(TenantProvider.GetTenantId());
        var equals = Expression.Equal(tenantIdProp, tenantIdValue);
        return Expression.Lambda<Func<T, bool>>(equals, param);
    }
}
