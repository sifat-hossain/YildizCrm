using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Yildiz.CRM.Applications.Actions.Policies.Queries;
using Yildiz.CRM.Applications.Actions.Policies.Queries.GetCustomerPolicy;
using Yildiz.CRM.Applications.Actions.Policies.Queries.GetExpirePolicy;

namespace Yildiz.CRM.Api.Controllers;

[ApiController]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public class PoliciesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PoliciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("api/customers/{customerId}/policy")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<List<PolicyModel>> GetCustomerPolicies([FromRoute] Guid customerId)
    {
        var command = new GetCustomerPoliciesQuery(customerId);
        return await _mediator.Send(command);
    }

    [HttpGet("api/policies/expiring")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<List<PolicyModel>> GetExpiringPolicies([FromQuery] int withinDays = 30)
    {

        var command = new GetExpiringPoliciesQuery(withinDays);
        return await _mediator.Send(command);
    }
}
