using MediatR;

namespace Yildiz.CRM.Applications.Actions.Policies.Queries.GetExpirePolicy;

public record GetExpiringPoliciesQuery(int WithinDays) : IRequest<List<PolicyModel>>;
