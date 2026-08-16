using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Yildiz.CRM.Applications.Actions.Auth.Commands;

namespace Yildiz.CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Simple login endpoint - No parameters required.
    /// Returns a JWT token with customer and tenant information.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login()
    {
        try
        {
            var command = new LoginCommand();
            var response = await _mediator.Send(command);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Login Failed",
                detail: ex.Message
            );
        }
    }
}
