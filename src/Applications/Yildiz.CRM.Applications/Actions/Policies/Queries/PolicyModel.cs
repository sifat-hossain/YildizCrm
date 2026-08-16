using System.Linq.Expressions;
using Yildiz.CRM.Domains.Entities;

namespace Yildiz.CRM.Applications.Actions.Policies.Queries;

public class PolicyModel
{
    public Guid CustomerId { get; set; }
    public required string PolicyNumber { get; set; }
    public DateTime ExpirationDate { get; set; }
    public decimal PremiumAmount { get; set; }

    public static Expression<Func<Policy, PolicyModel>> Projection => policy => new PolicyModel
    {
        CustomerId = policy.CustomerId,
        PolicyNumber = policy.PolicyNumber,
        ExpirationDate = policy.ExpirationDate,
        PremiumAmount = policy.PremiumAmount
    };
}
