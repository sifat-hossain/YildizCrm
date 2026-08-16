using Microsoft.EntityFrameworkCore;
using Moq;
using Yildiz.CRM.Applications.Actions.Policies.Queries.GetCustomerPolicy;
using Yildiz.CRM.Applications.Interfaces;
using Yildiz.CRM.Domains.Entities;
using Yildz.CRM.Infrastructures;

namespace Yildiz.CRM.Tests.Unit;

public class GetCustomerPoliciesQueryHandlerTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<IIdentityContext> _identityContextMock;
    private readonly GetCustomerPoliciesQueryHandler _handler;

    public GetCustomerPoliciesQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;

        _dbContext = new CrmDbContext(options);
        _identityContextMock = new Mock<IIdentityContext>();
        _handler = new GetCustomerPoliciesQueryHandler(_dbContext, _identityContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCustomerInSameTenant_ReturnsPolicies()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var tenant = new Tenant { Id = tenantId, Name = "Test Tenant" };
        var customer = new Customer { Id = customerId, TenantId = tenantId, Name = "John Doe" };
        var policy1 = new Policy { Id = Guid.NewGuid(), CustomerId = customerId, PolicyNumber = "POL-001", ExpirationDate = DateTime.UtcNow.AddMonths(6), PremiumAmount = 1000m };
        var policy2 = new Policy { Id = Guid.NewGuid(), CustomerId = customerId, PolicyNumber = "POL-002", ExpirationDate = DateTime.UtcNow.AddMonths(12), PremiumAmount = 1500m };

        _dbContext.Tenants.Add(tenant);
        _dbContext.Customers.Add(customer);
        _dbContext.Policies.AddRange(policy1, policy2);
        await _dbContext.SaveChangesAsync();

        _identityContextMock.Setup(x => x.TenantId).Returns(tenantId);

        var query = new GetCustomerPoliciesQuery(customerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.PolicyNumber == "POL-001");
        Assert.Contains(result, p => p.PolicyNumber == "POL-002");
        Assert.All(result, p => Assert.Equal(customerId, p.CustomerId));
    }

    [Fact]
    public async Task Handle_WithCustomerFromDifferentTenant_ThrowsUnauthorizedAccessException()
    {
        // Arrange - Set up two different tenants
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();
        var customer1Id = Guid.NewGuid();

        var tenant1 = new Tenant { Id = tenant1Id, Name = "Tenant 1" };
        var tenant2 = new Tenant { Id = tenant2Id, Name = "Tenant 2" };
        var customer1 = new Customer { Id = customer1Id, TenantId = tenant1Id, Name = "Customer from Tenant 1" };
        var policy1 = new Policy { Id = Guid.NewGuid(), CustomerId = customer1Id, PolicyNumber = "POL-001", ExpirationDate = DateTime.UtcNow.AddMonths(6), PremiumAmount = 1000m };

        _dbContext.Tenants.AddRange(tenant1, tenant2);
        _dbContext.Customers.Add(customer1);
        _dbContext.Policies.Add(policy1);
        await _dbContext.SaveChangesAsync();

        // User is authenticated as Tenant 2, but tries to access Tenant 1's customer
        _identityContextMock.Setup(x => x.TenantId).Returns(tenant2Id);

        var query = new GetCustomerPoliciesQuery(customer1Id);

        // Act & Assert - Should throw because customer belongs to different tenant
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(query, CancellationToken.None));

        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ThrowsKeyNotFoundException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var nonExistentCustomerId = Guid.NewGuid();

        _identityContextMock.Setup(x => x.TenantId).Returns(tenantId);

        var query = new GetCustomerPoliciesQuery(nonExistentCustomerId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(query, CancellationToken.None));

        Assert.Contains(nonExistentCustomerId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_WithoutTenantContext_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _identityContextMock.Setup(x => x.TenantId).Returns((Guid?)null);

        var query = new GetCustomerPoliciesQuery(customerId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(query, CancellationToken.None));

        Assert.Contains("Tenant context is required", exception.Message);
    }

    [Fact]
    public async Task Handle_WithValidCustomer_ReturnsEmptyListWhenNoPolicies()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var tenant = new Tenant { Id = tenantId, Name = "Test Tenant" };
        var customer = new Customer { Id = customerId, TenantId = tenantId, Name = "John Doe" };

        _dbContext.Tenants.Add(tenant);
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        _identityContextMock.Setup(x => x.TenantId).Returns(tenantId);

        var query = new GetCustomerPoliciesQuery(customerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}
