# Running Yildiz CRM Locally

A step-by-step guide to get the Yildiz CRM API running on your local development machine.

## Prerequisites

Before you begin, ensure you have the following installed:

### Required
- **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** - Latest version
- **[SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)** - Express Edition or LocalDB is sufficient
- **[Visual Studio 2026](https://visualstudio.microsoft.com/)** (18.9.0+) or **[Visual Studio Code](https://code.visualstudio.com/)** with C# extension

### Optional (Recommended)
- **[SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)** - For database management
- **[Postman](https://www.postman.com/)** or **[Thunder Client](https://www.thunderclient.com/)** - For API testing
- **Git** - For version control

---

## Step 1: Clone or Open the Project

### If using Git:
```bash
git clone <repository-url>
cd YildizCRM
```

### If you already have the project:
- Open the solution file: `YildizCRM.slnx`
- Visual Studio will restore NuGet packages automatically

---

## Step 2: Configure Database Connection

### Option A: Using SQL Server LocalDB (Recommended for Development)

1. Open `src/Presentations/Yildiz.CRM.Api/appsettings.json`

2. Update the connection string:
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=YildizCrmDb;Trusted_Connection=true;TrustServerCertificate=true"
  }
}
```

## Step 3: Configure JWT Settings

The JWT configuration is already set in `appsettings.json`. For local development, the defaults work fine:

```json
{
  "JwtSettings": {
	"SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
	"Issuer": "YildizCRM",
	"Audience": "YildizCRMUsers",
	"ExpirationMinutes": 60
  }
}
```

## Step 4: Apply Database Migrations

The database will migrate and seed data automatically when you run the application. However, you can also apply migrations manually.

### Option A: Using Visual Studio Package Manager Console

1. Open **Tools → NuGet Package Manager → Package Manager Console**

2. Ensure the **Default project** dropdown is set to `src\Infrastructures\Yildz.CRM.Infrastructures`

3. Run the migration command:
```powershell
Update-Database
```

**Alternative with explicit project paths:**
```powershell
Update-Database -Project src\Infrastructures\Yildz.CRM.Infrastructures -StartupProject src\Presentations\Yildiz.CRM.Api
```

**Expected Output:**
```
Build started...
Build succeeded.
Applying migration '20250101000000_InitialCreate'.
Done.
```

---

### Verify Migration Success

**Check database exists:**

**SQL Server LocalDB:**
```bash
sqllocaldb info mssqllocaldb
```

**Using SQL Server Management Studio (SSMS):**
1. Connect to your SQL Server instance
2. Look for the `YildizCrmDb` database
3. Expand **Tables** - you should see:
   - `Tenants`
   - `Customers`
   - `Policies`
   - `__EFMigrationsHistory`

**Query seed data:**
```sql
SELECT COUNT(*) FROM Tenants;    -- Should return 2
SELECT COUNT(*) FROM Customers;  -- Should return 4
SELECT COUNT(*) FROM Policies;   -- Should return 5
```

---

## Step 5: Build the Solution

### Using Visual Studio:
- Press `Ctrl + Shift + B` or
- **Build → Build Solution**

### Using CLI:
```bash
dotnet build
```

You should see: **Build succeeded. 0 Warning(s). 0 Error(s).**

---

## Step 6: Run the Application

### Using Visual Studio:
1. Set `Yildiz.CRM.Api` as the startup project (right-click → Set as Startup Project)
2. Press `F5` to run with debugging, or `Ctrl + F5` to run without debugging
3. The API will launch and your browser will open to `/scalar/v1`

### Using CLI:
```bash
cd src/Presentations/Yildiz.CRM.Api
dotnet run
```

### Expected Output:
```
info: Microsoft.Hosting.Lifetime[14]
	  Now listening on: https://localhost:7265
info: Microsoft.Hosting.Lifetime[14]
	  Now listening on: http://localhost:5104
info: Microsoft.Hosting.Lifetime[0]
	  Application started. Press Ctrl+C to shut down.
```

---

## Step 7: Access the API Documentation

Once the application is running, navigate to:

**Scalar UI (Interactive API Docs):**
```
https://localhost:7265/scalar/
```
---

## Step 8: Test the API

### 1. Get an Authentication Token

**Endpoint:** `POST /api/auth/login`

**Using cURL:**
```bash
curl -X POST https://localhost:7265/api/auth/login
```

**Using PowerShell:**
```powershell
Invoke-RestMethod -Uri "https://localhost:7265/api/auth/login" -Method Post
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

Copy the token value - you'll need it for authenticated requests.

---

### 2. Get Customer Policies (Authenticated)

**Endpoint:** `GET /api/customers/{customerId}/policy`

**Example Customer ID:** `00000000-0000-0000-0000-000000000011`

**Using cURL:**
```bash
curl -X GET "https://localhost:7265/api/customers/00000000-0000-0000-0000-000000000011/policy" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**Using PowerShell:**
```powershell
$token = "YOUR_TOKEN_HERE"
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri "https://localhost:7265/api/customers/00000000-0000-0000-0000-000000000011/policy" -Headers $headers
```

**Expected Response:**
```json
[
  {
	"customerId": "00000000-0000-0000-0000-000000000011",
	"policyNumber": "POL-001",
	"expirationDate": "2026-08-15T00:00:00Z",
	"premiumAmount": 1200.00
  },
  {
	"customerId": "00000000-0000-0000-0000-000000000011",
	"policyNumber": "POL-002",
	"expirationDate": "2026-09-15T00:00:00Z",
	"premiumAmount": 850.00
  }
]
```

---

### 3. Get Expiring Policies (Authenticated)

**Endpoint:** `GET /api/policies/expiring?withinDays=30`

**Using cURL:**
```bash
curl -X GET "https://localhost:7265/api/policies/expiring?withinDays=365" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**Using PowerShell:**
```powershell
$token = "YOUR_TOKEN_HERE"
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri "https://localhost:7265/api/policies/expiring?withinDays=365" -Headers $headers
```

---

## Step 9: Run Unit Tests

### Using Visual Studio:
1. Open **Test → Test Explorer**
2. Click **Run All Tests**
3. View results in Test Explorer

### Using CLI:
```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~GetCustomerPoliciesQueryHandlerTests"

# Run with detailed output
dotnet test --verbosity detailed
```

**Expected Output:**
```
Passed!  - Failed:     0, Passed:    11, Skipped:     0, Total:    11
```

---

## Seed Data

The application includes seed data for testing:

### Tenants:
- **Tenant 1:** `00000000-0000-0000-0000-000000000001` (Acme Corporation)
- **Tenant 2:** `00000000-0000-0000-0000-000000000002` (GlobalTech Industries)

### Customers:
- **John Doe** (Tenant 1): `00000000-0000-0000-0000-000000000011`
- **Jane Smith** (Tenant 1): `00000000-0000-0000-0000-000000000012`
- **Bob Johnson** (Tenant 2): `00000000-0000-0000-0000-000000000021`
- **Alice Williams** (Tenant 2): `00000000-0000-0000-0000-000000000022`

### Policies:
- Each customer has 1-2 policies with expiration dates in 2026

---

## Troubleshooting

### Issue: Database connection fails
**Solution:** 
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure LocalDB is installed: `sqllocaldb info`

### Issue: Migrations not applying
**Solution:**
```bash
cd src/Presentations/Yildiz.CRM.Api
dotnet ef database drop --project ../../Infrastructures/Yildz.CRM.Infrastructures
dotnet ef database update --project ../../Infrastructures/Yildz.CRM.Infrastructures
```

### Issue: Port already in use
**Solution:**
- Change ports in `launchSettings.json`:
  ```json
  "applicationUrl": "https://localhost:7265;http://localhost:5104"
  ```

### Issue: JWT token validation fails
**Solution:**
- Check `JwtSettings` in `appsettings.json` match between token generation and validation
- Ensure token hasn't expired (default 60 minutes)

### Issue: Scalar UI not showing endpoints
**Solution:**
- Ensure you're navigating to `/scalar/`
- Clear browser cache
- Check that XML documentation generation is enabled in `.csproj`

### Issue: NuGet package restore fails
**Solution:**
```bash
dotnet restore
dotnet clean
dotnet build
```

---

## Project Structure

```
YildizCRM/
├── src/
│   ├── Presentations/
│   │   └── Yildiz.CRM.Api/          # Web API project (controllers, Program.cs)
│   ├── Applications/
│   │   └── Yildiz.CRM.Applications/ # CQRS handlers, DTOs, interfaces
│   ├── Infrastructures/
│   │   └── Yildz.CRM.Infrastructures/ # EF Core, DbContext, migrations
│   └── Domains/
│       └── Yildiz.CRM.Domains/      # Domain entities
├── tests/
│   └── Yildiz.CRM.Tests/            # Unit and integration tests
└── YildizCRM.slnx                   # Solution file
```

---


## Next Steps

- ✅ Read `TENANT_ISOLATION.md` to understand the security architecture
- ✅ Explore the Scalar UI at `/scalar/v1`
- ✅ Run the unit tests to see tenant isolation in action
- ✅ Try making cross-tenant requests to see security in action
- ✅ Modify seed data in `Configurations` folder if needed

---

## Support

For issues or questions:
1. Check the **Troubleshooting** section above
2. Review the unit tests for usage examples
3. Examine the Scalar API documentation at `/scalar/v1`
4. Check application logs in the console output

---

**Happy Coding! 🚀**
