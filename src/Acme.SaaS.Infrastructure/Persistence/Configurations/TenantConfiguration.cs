using Acme.SaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.SaaS.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => t.Identifier).IsUnique();
        builder.Property(t => t.Identifier).IsRequired().HasMaxLength(100);
        builder.Property(t => t.SchemaName).HasMaxLength(200);
        builder.Property(t => t.ConnectionString).HasMaxLength(500);
        builder.Property(t => t.Plan).HasConversion<string>().HasMaxLength(50);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(50);
    }
}
