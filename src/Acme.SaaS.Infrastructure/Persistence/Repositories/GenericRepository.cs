using System.Linq.Expressions;
using Acme.SaaS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Acme.SaaS.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly DbContext Context;
    protected readonly DbSet<T> Set;

    public GenericRepository(IApplicationDbContext context)
    {
        Context = (DbContext)context;
        Set = Context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await Set.FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await Set.ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await Set.Where(predicate).ToListAsync(ct);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await Set.AnyAsync(predicate, ct);

    public void Add(T entity) => Set.Add(entity);

    public void Update(T entity) => Set.Update(entity);

    public void Delete(T entity) => Set.Remove(entity);
}
