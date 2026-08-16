using MediatR;
using Microsoft.EntityFrameworkCore;
using Yildiz.CRM.Applications.Interfaces;

namespace Yildiz.CRM.Applications.Actions.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginModel>
{
    private readonly ICrmDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(ICrmDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginModel> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Get first customer (simplified for demo - no authentication required)
        var customer = await _context.Customers
            .Include(c => c.Tenant)
            .FirstOrDefaultAsync(cancellationToken);

        if (customer == null)
        {
            throw new InvalidOperationException("No customer found in the system");
        }

        // Generate JWT token with customer and tenant info
        var token = _jwtTokenService.GenerateToken(customer.Id, customer.TenantId, customer.Name);

        return new LoginModel
        {
            Token = token
        };
    }
}
