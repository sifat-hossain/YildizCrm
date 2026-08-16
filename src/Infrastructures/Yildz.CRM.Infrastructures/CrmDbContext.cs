using Base.Domains;
using Microsoft.EntityFrameworkCore;
using Yildiz.CRM.Applications.Interfaces;
using Yildiz.CRM.Domains.Entities;

namespace Yildz.CRM.Infrastructures;

public class CrmDbContext(DbContextOptions<CrmDbContext> options) :
    DbContext(options), ICrmDbContext
{
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Policy> Policies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasKey(nameof(BaseEntity.Id));

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.Id))
                    .HasDefaultValueSql("NEWID()");

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.CreatedAt))
                    .IsRequired(false);

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.UpdatedAt))
                    .IsRequired(false);

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.CreatedBy))
                    .IsRequired(false);

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.UpdatedBy))
                    .IsRequired(false);

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.IsDeleted))
                    .HasDefaultValue(false);

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.IsActive))
                    .HasDefaultValue(true);

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(BaseEntity.IsDeleted));

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(BaseEntity.IsActive));

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(BaseEntity.CreatedAt));
            }
        }

        // Apply entity-specific configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
    }
}
