using Base.Domains;

namespace Yildiz.CRM.Domains.Entities;

public class Policy : BaseEntity
{
    public Guid CustomerId { get; set; }
    public required string PolicyNumber { get; set; }
    public DateTime ExpirationDate { get; set; }
    public decimal PremiumAmount { get; set; }
    public Customer? Customer { get; set; }
}
