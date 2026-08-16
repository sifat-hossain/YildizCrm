using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yildiz.CRM.Domains.Entities;

namespace Yildz.CRM.Infrastructures.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        // Entity-specific configurations only
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(e => e.TenantId);

        builder.HasOne(e => e.Tenant)
            .WithMany(t => t.Customers)
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed data
        var tenant1Id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var tenant2Id = Guid.Parse("00000000-0000-0000-0000-000000000002");

        builder.HasData(
            new Customer { Id = Guid.Parse("00000000-0000-0000-0000-000000000011"), TenantId = tenant1Id, Name = "John Doe" },
            new Customer { Id = Guid.Parse("00000000-0000-0000-0000-000000000012"), TenantId = tenant1Id, Name = "Jane Smith" },
            new Customer { Id = Guid.Parse("00000000-0000-0000-0000-000000000021"), TenantId = tenant2Id, Name = "Bob Johnson" },
            new Customer { Id = Guid.Parse("00000000-0000-0000-0000-000000000022"), TenantId = tenant2Id, Name = "Alice Williams" }
        );
    }
}
