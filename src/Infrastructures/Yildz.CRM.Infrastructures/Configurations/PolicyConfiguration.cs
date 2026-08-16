using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yildiz.CRM.Domains.Entities;

namespace Yildz.CRM.Infrastructures.Configurations;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.Property(e => e.PolicyNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.PremiumAmount)
            .HasPrecision(18, 2);

        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.ExpirationDate);

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Policies)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed data - Using customer IDs from CustomerConfiguration
        var customer1Id = Guid.Parse("00000000-0000-0000-0000-000000000011");
        var customer2Id = Guid.Parse("00000000-0000-0000-0000-000000000012");
        var customer3Id = Guid.Parse("00000000-0000-0000-0000-000000000021");
        var customer4Id = Guid.Parse("00000000-0000-0000-0000-000000000022");

        // Use static dates for seed data (avoiding DateTime.UtcNow)
        builder.HasData(
            new Policy { Id = Guid.Parse("00000000-0000-0000-0000-000000000111"), CustomerId = customer1Id, PolicyNumber = "POL-001", ExpirationDate = new DateTime(2026, 8, 15, 0, 0, 0, DateTimeKind.Utc), PremiumAmount = 1200.00m },
            new Policy { Id = Guid.Parse("00000000-0000-0000-0000-000000000112"), CustomerId = customer1Id, PolicyNumber = "POL-002", ExpirationDate = new DateTime(2026, 9, 15, 0, 0, 0, DateTimeKind.Utc), PremiumAmount = 850.00m },
            new Policy { Id = Guid.Parse("00000000-0000-0000-0000-000000000113"), CustomerId = customer2Id, PolicyNumber = "POL-003", ExpirationDate = new DateTime(2026, 8, 25, 0, 0, 0, DateTimeKind.Utc), PremiumAmount = 1500.00m },
            new Policy { Id = Guid.Parse("00000000-0000-0000-0000-000000000211"), CustomerId = customer3Id, PolicyNumber = "POL-004", ExpirationDate = new DateTime(2026, 9, 10, 0, 0, 0, DateTimeKind.Utc), PremiumAmount = 2000.00m },
            new Policy { Id = Guid.Parse("00000000-0000-0000-0000-000000000212"), CustomerId = customer4Id, PolicyNumber = "POL-005", ExpirationDate = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc), PremiumAmount = 950.00m }
        );
    }
}
