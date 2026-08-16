using MediatR;
using Microsoft.EntityFrameworkCore;
using Yildiz.CRM.Applications.Interfaces;

namespace Yildiz.CRM.Applications.Actions.Policies.Queries.GetExpirePolicy;

public class GetExpiringPoliciesQueryHandler(ICrmDbContext context, IIdentityContext identityContext) : IRequestHandler<GetExpiringPoliciesQuery, List<PolicyModel>>
{
    public async Task<List<PolicyModel>> Handle(GetExpiringPoliciesQuery request, CancellationToken cancellationToken)
    {
        if (identityContext.TenantId == null)
        {
            throw new UnauthorizedAccessException("Tenant context is required");
        }

        var expirationDate = DateTime.UtcNow.AddDays(request.WithinDays);

        var policies = await context.Policies
            .Where(p => p.Customer.TenantId == identityContext.TenantId &&
                       p.ExpirationDate <= expirationDate &&
                       p.ExpirationDate >= DateTime.UtcNow)
            .OrderBy(p => p.ExpirationDate)
            .Select(policy => new PolicyModel
            {
                CustomerId = policy.CustomerId,
                PolicyNumber = policy.PolicyNumber,
                ExpirationDate = policy.ExpirationDate,
                PremiumAmount = policy.PremiumAmount
            })
            .ToListAsync(cancellationToken);

        return policies;
    }
}
