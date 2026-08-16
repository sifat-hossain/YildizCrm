using MediatR;

namespace Yildiz.CRM.Applications.Actions.Policies.Queries.GetCustomerPolicy;

public record GetCustomerPoliciesQuery(Guid CustomerId) : IRequest<List<PolicyModel>>;
