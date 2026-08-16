using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Yildiz.CRM.Applications.Interfaces;

namespace Yildz.CRM.Infrastructures.Services;

public class IdentityContextService : IIdentityContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("TenantId")?.Value;
            return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
        }
    }

    public Guid? CustomerId
    {
        get
        {
            var customerIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("CustomerId")?.Value;
            return Guid.TryParse(customerIdClaim, out var customerId) ? customerId : null;
        }
    }

    public string? CustomerName
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst("CustomerName")?.Value;
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }
}
