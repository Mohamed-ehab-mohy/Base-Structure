using Acme.SaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Acme.SaaS.Infrastructure.Persistence.Contexts;

public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.Identifier).IsUnique();
            entity.Property(t => t.Identifier).IsRequired().HasMaxLength(100);
            entity.Property(t => t.SchemaName).HasMaxLength(200);
        });
    }
}
