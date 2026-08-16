namespace Yildiz.CRM.Applications.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(Guid customerId, Guid tenantId, string customerName);
}
