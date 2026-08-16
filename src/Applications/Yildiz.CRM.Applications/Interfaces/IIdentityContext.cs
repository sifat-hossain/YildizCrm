namespace Yildiz.CRM.Applications.Interfaces;

public interface IIdentityContext
{
    Guid? TenantId { get; }
    Guid? CustomerId { get; }
    string? CustomerName { get; }
    bool IsAuthenticated { get; }
}
