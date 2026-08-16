using Microsoft.EntityFrameworkCore;
using Moq;
using Yildiz.CRM.Applications.Actions.Policies.Queries.GetExpirePolicy;
using Yildiz.CRM.Applications.Interfaces;
using Yildiz.CRM.Domains.Entities;
using Yildz.CRM.Infrastructures;

namespace Yildiz.CRM.Tests.Unit;

public class GetExpiringPoliciesQueryHandlerTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<IIdentityContext> _identityContextMock;
    private readonly GetExpiringPoliciesQueryHandler _handler;

    public GetExpiringPoliciesQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;

        _dbContext = new CrmDbContext(options);
        _identityContextMock = new Mock<IIdentityContext>();
        _handler = new GetExpiringPoliciesQueryHandler(_dbContext, _identityContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithExpiringPoliciesInSameTenant_ReturnsOnlyTenantPolicies()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var tenant = new Tenant { Id = tenantId, Name = "Test Tenant" };
        var customer = new Customer { Id = customerId, TenantId = tenantId, Name = "John Doe" };

        // Policy expiring in 15 days
        var expiringPolicy1 = new Policy
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PolicyNumber = "POL-001",
            ExpirationDate = DateTime.UtcNow.AddDays(15),
            PremiumAmount = 1000m
        };

        // Policy expiring in 25 days
        var expiringPolicy2 = new Policy
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PolicyNumber = "POL-002",
            ExpirationDate = DateTime.UtcNow.AddDays(25),
            PremiumAmount = 1500m
        };

        // Policy expiring in 35 days (should not be returned)
        var notExpiringPolicy = new Policy
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PolicyNumber = "POL-003",
            ExpirationDate = DateTime.UtcNow.AddDays(35),
            PremiumAmount = 2000m
        };

        _dbContext.Tenants.Add(tenant);
        _dbContext.Customers.Add(customer);
        _dbContext.Policies.AddRange(expiringPolicy1, expiringPolicy2, notExpiringPolicy);
        await _dbContext.SaveChangesAsync();

        _identityContextMock.Setup(x => x.TenantId).Returns(tenantId);

        var query = new GetExpiringPoliciesQuery(30);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.PolicyNumber == "POL-001");
        Assert.Contains(result, p => p.PolicyNumber == "POL-002");
        Assert.DoesNotContain(result, p => p.PolicyNumber == "POL-003");
    }

    [Fact]
    public async Task Handle_WithMultipleTenants_BlocksCrossTenantAccess()
    {
        // Arrange - Create two tenants with their own policies
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();
        var customer1Id = Guid.NewGuid();
        var customer2Id = Guid.NewGuid();

        var tenant1 = new Tenant { Id = tenant1Id, Name = "Tenant 1" };
        var tenant2 = new Tenant { Id = tenant2Id, Name = "Tenant 2" };
        var customer1 = new Customer { Id = customer1Id, TenantId = tenant1Id, Name = "Customer 1" };
        var customer2 = new Customer { Id = customer2Id, TenantId = tenant2Id, Name = "Customer 2" };

        // Tenant 1 policy expiring in 15 days
        var tenant1Policy = new Policy
        {
            Id = Guid.NewGuid(),
            CustomerId = customer1Id,
            PolicyNumber = "TENANT1-POL-001",
            ExpirationDate = DateTime.UtcNow.AddDays(15),
            PremiumAmount = 1000m
        };

        // Tenant 2 policy expiring in 20 days
        var tenant2Policy = new Policy
        {
            Id = Guid.NewGuid(),
            CustomerId = customer2Id,
            PolicyNumber = "TENANT2-POL-001",
            ExpirationDate = DateTime.UtcNow.AddDays(20),
            PremiumAmount = 1500m
        };

        _dbContext.Tenants.AddRange(tenant1, tenant2);
        _dbContext.Customers.AddRange(customer1, customer2);
        _dbContext.Policies.AddRange(tenant1Policy, tenant2Policy);
        await _dbContext.SaveChangesAsync();

        // Authenticate as Tenant 1
        _identityContextMock.Setup(x => x.TenantId).Returns(tenant1Id);

        var query = new GetExpiringPoliciesQuery(30);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - Should only see Tenant 1's policies
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains(result, p => p.PolicyNumber == "TENANT1-POL-001");
        Assert.DoesNotContain(result, p => p.PolicyNumber == "TENANT2-POL-001");
    }

    [Fact]
    public async Task Handle_WithoutTenantContext_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _identityContextMock.Setup(x => x.TenantId).Returns((Guid?)null);

        var query = new GetExpiringPoliciesQuery(30);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(query, CancellationToken.None));

        Assert.Contains("Tenant context is required", exception.Message);
    }

    [Fact]
    public async Task Handle_WithExpiredPolicies_ExcludesAlreadyExpiredPolicies()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var tenant = new Tenant { Id = tenantId, Name = "Test Tenant" };
        var customer = new Customer { Id = customerId, TenantId = tenantId, Name = "John Doe" };

        // Policy expiring in 10 days
        var expiringPolicy = new Policy
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PolicyNumber = "POL-001",
            ExpirationDate = DateTime.UtcNow.AddDays(10),
            PremiumAmount = 1000m
        };

        // Policy already expired (5 days ago)
        var expiredPolicy = new Policy
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PolicyNumber = "POL-002",
            ExpirationDate = DateTime.UtcNow.AddDays(-5),
            PremiumAmount = 1500m
        };

        _dbContext.Tenants.Add(tenant);
        _dbContext.Customers.Add(customer);
        _dbContext.Policies.AddRange(expiringPolicy, expiredPolicy);
        await _dbContext.SaveChangesAsync();

        _identityContextMock.Setup(x => x.TenantId).Returns(tenantId);

        var query = new GetExpiringPoliciesQuery(30);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains(result, p => p.PolicyNumber == "POL-001");
        Assert.DoesNotContain(result, p => p.PolicyNumber == "POL-002");
    }

    [Fact]
    public async Task Handle_WithNoPolicies_ReturnsEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var tenant = new Tenant { Id = tenantId, Name = "Test Tenant" };

        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync();

        _identityContextMock.Setup(x => x.TenantId).Returns(tenantId);

        var query = new GetExpiringPoliciesQuery(30);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_OrdersByExpirationDate_Ascending()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var tenant = new Tenant { Id = tenantId, Name = "Test Tenant" };
        var customer = new Customer { Id = customerId, TenantId = tenantId, Name = "John Doe" };

        var policy1 = new Policy
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PolicyNumber = "POL-001",
            ExpirationDate = DateTime.UtcNow.AddDays(20),
            PremiumAmount = 1000m
        };

        var policy2 = new Policy
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PolicyNumber = "POL-002",
            ExpirationDate = DateTime.UtcNow.AddDays(10),
            PremiumAmount = 1500m
        };

        var policy3 = new Policy
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PolicyNumber = "POL-003",
            ExpirationDate = DateTime.UtcNow.AddDays(15),
            PremiumAmount = 2000m
        };

        _dbContext.Tenants.Add(tenant);
        _dbContext.Customers.Add(customer);
        _dbContext.Policies.AddRange(policy1, policy2, policy3);
        await _dbContext.SaveChangesAsync();

        _identityContextMock.Setup(x => x.TenantId).Returns(tenantId);

        var query = new GetExpiringPoliciesQuery(30);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("POL-002", result[0].PolicyNumber); // Expires first (10 days)
        Assert.Equal("POL-003", result[1].PolicyNumber); // Expires second (15 days)
        Assert.Equal("POL-001", result[2].PolicyNumber); // Expires last (20 days)
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}
