using Base.Domains;

namespace Yildiz.CRM.Domains.Entities;

public class Tenant : BaseEntity
{
    public required string Name { get; set; }
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
