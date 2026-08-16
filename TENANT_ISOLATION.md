# Tenant Isolation Strategy

## Overview

This CRM system implements **strict tenant isolation** to ensure that customers and policies from one tenant can never be accessed by users from another tenant, even if valid IDs are provided.

## How Tenant Isolation is Enforced

### 1. **Identity Context Abstraction**
- Created `IIdentityContext` service that extracts `TenantId` and `CustomerId` from JWT claims
- Every authenticated request automatically includes tenant context from the token
- Handlers inject `IIdentityContext` instead of accessing HTTP concerns directly

### 2. **Query-Level Enforcement**
Both policy query handlers implement tenant checks at multiple levels:

**GetCustomerPoliciesQueryHandler:**
```csharp
// Step 1: Verify tenant context exists
if (_identityContext.TenantId == null)
	throw new UnauthorizedAccessException("Tenant context is required");

// Step 2: Verify customer belongs to requesting tenant
var customer = await _context.Customers
	.Where(c => c.Id == customerId && c.TenantId == _identityContext.TenantId)
	.FirstOrDefaultAsync();

// Step 3: Reject cross-tenant access
if (customer == null)
	throw new KeyNotFoundException("Customer not found");
```

**GetExpiringPoliciesQueryHandler:**
```csharp
// Filter policies by tenant via navigation property
var policies = await _context.Policies
	.Where(p => p.Customer.TenantId == identityContext.TenantId)
	.ToListAsync();
```

### 3. **Database-Level Relationships**
- Every `Customer` has a `TenantId` foreign key
- Every `Policy` has a `CustomerId` foreign key to `Customer`
- Navigation properties (`Policy.Customer.TenantId`) enable tenant filtering in queries

### 4. **JWT Token Design**
- Login endpoint issues tokens containing `TenantId` and `CustomerId` claims
- All protected endpoints require `[Authorize]` attribute
- Token validation ensures claims integrity

## Why This Approach

✅ **Defense in Depth** - Multiple layers prevent accidental data leaks  
✅ **Explicit Verification** - Every query explicitly checks tenant ownership  
✅ **Fail-Safe** - Missing tenant context throws error rather than exposing all data  
✅ **Testable** - `IIdentityContext` can be mocked for unit testing  
✅ **Clear Separation** - Application layer has no HTTP dependencies

## Production Scale Improvements

For real production deployment with large datasets, the following changes would be recommended:

### 1. **Global Query Filters**
```csharp
// In DbContext.OnModelCreating
modelBuilder.Entity<Policy>().HasQueryFilter(p => 
	p.Customer.TenantId == _identityContext.TenantId);
```
- Automatically applies tenant filter to ALL queries
- Reduces risk of developer error
- Centralized enforcement point

### 2. **Database Partitioning**
- Physical tenant isolation using schema-per-tenant or database-per-tenant
- Improved query performance for large tenants
- Enhanced security through physical separation

### 3. **Row-Level Security (RLS)**
- Database-enforced tenant filtering using SQL Server RLS
- Protection even if application code has bugs
- `SESSION_CONTEXT` sets tenant ID per connection

### 4. **Caching Strategy**
- Redis/Memcached with tenant-scoped keys (`tenant:{tenantId}:policy:{policyId}`)
- Prevent cache poisoning across tenants

### 5. **Logging**
- Log every cross-tenant access attempt (even failed ones)
- Track which user attempted to access which tenant's data
- Compliance and forensics requirement

### 6. **Request Rate Limiting**
- Per-tenant rate limits to prevent noisy neighbors
- Distributed rate limiting (Redis-based) for horizontal scaling
- Protect against tenant-level DoS attacks

### 7. **Connection Pooling & Multi-Tenancy**
- Connection pool per tenant for large tenants
- Shared pool for smaller tenants
- Dynamic scaling based on tenant activity

### 8. **Data Encryption**
- Transparent Data Encryption (TDE) at rest
- Tenant-specific encryption keys for highly sensitive data
- Azure Key Vault or AWS KMS integration

## Testing Strategy

**Unit Tests** verify tenant isolation:
- `Handle_WithCustomerFromDifferentTenant_ThrowsUnauthorizedAccessException` - Proves Tenant 2 cannot access Tenant 1 data
- `Handle_WithMultipleTenants_BlocksCrossTenantAccess` - Proves expiring policies are scoped per tenant
- Both use in-memory databases and mock identity context for fast, isolated testing

## Running the Application

1. **Get a JWT token:**
   ```
   POST /api/auth/login
   ```

2. **Use token in requests:**
   ```
   GET /api/customers/{customerId}/policy
   Authorization: Bearer {token}
   ```

3. **View API docs:**
   Navigate to `/scalar/v1` in browser

## Security Notes

⚠️ **Never trust route parameters alone** - Always verify tenant ownership  
⚠️ **Token validation must be strict** - Use secure signing keys  
⚠️ **Audit all access** - Log tenant context with every operation  
⚠️ **Test cross-tenant scenarios** - Unit tests must prove isolation
