using Microsoft.EntityFrameworkCore;
using Acme.SaaS.Domain.Entities;

namespace Acme.SaaS.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<Product> Products { get; }
    DbSet<Role> Roles { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
