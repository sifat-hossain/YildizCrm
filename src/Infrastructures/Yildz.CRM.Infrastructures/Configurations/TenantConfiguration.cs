using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yildiz.CRM.Domains.Entities;

namespace Yildz.CRM.Infrastructures.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        // Entity-specific configurations only
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Seed data
        builder.HasData(
            new Tenant { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "Acme Corporation" },
            new Tenant { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "GlobalTech Industries" }
        );
    }
}
