using MediatR;
using Microsoft.EntityFrameworkCore;
using Yildiz.CRM.Applications.Interfaces;

namespace Yildiz.CRM.Applications.Actions.Policies.Queries.GetCustomerPolicy;

public class GetCustomerPoliciesQueryHandler : IRequestHandler<GetCustomerPoliciesQuery, List<PolicyModel>>
{
    private readonly ICrmDbContext _context;
    private readonly IIdentityContext _identityContext;

    public GetCustomerPoliciesQueryHandler(ICrmDbContext context, IIdentityContext identityContext)
    {
        _context = context;
        _identityContext = identityContext;
    }

    public async Task<List<PolicyModel>> Handle(GetCustomerPoliciesQuery request, CancellationToken cancellationToken)
    {
        if (_identityContext.TenantId == null)
        {
            throw new UnauthorizedAccessException("Tenant context is required");
        }

        var customer = await _context.Customers
            .Where(c => c.Id == request.CustomerId && c.TenantId == _identityContext.TenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {request.CustomerId} not found");
        }

        var policies = await _context.Policies
            .Where(p => p.CustomerId == request.CustomerId)
            .OrderByDescending(p => p.CreatedAt)
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
