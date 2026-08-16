using Base.Domains;

namespace Yildiz.CRM.Domains.Entities;

public class Customer : BaseEntity
{
    public Guid TenantId { get; set; }
    public required string Name { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Policy> Policies { get; set; } = new List<Policy>();
}
