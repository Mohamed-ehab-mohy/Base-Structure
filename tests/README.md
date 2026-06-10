# Tests

**Location:** `tests/`

The solution contains four test projects, mirroring the source structure:

```
tests/
├── Acme.SaaS.Domain.Tests/         # Unit tests for Domain entities and logic
├── Acme.SaaS.Application.Tests/    # Tests for use cases and Mini-Services
├── Acme.SaaS.Infrastructure.Tests/ # Integration tests for data access
├── Acme.SaaS.API.Tests/            # Integration tests for API endpoints
└── README.md
```

## Test Project Details

### `Acme.SaaS.Domain.Tests`
Tests pure business logic with no dependencies:
- Entity behavior (e.g., product price validation)
- Value Object equality
- Enum conversions
- Domain exception throwing

**Framework**: xUnit
**Mocks**: None needed (pure logic)

### `Acme.SaaS.Application.Tests`
Tests application use cases with mocked infrastructure:
- Mini-Service logic (TenantService, AuthService, ProductService, BillingService)
- Validation rules
- AutoMapper mapping profiles
- ApiResponse and PagedResult behavior

**Framework**: xUnit + Moq (or NSubstitute)
**Mocks**: `IApplicationDbContext`, `ITenantProvider`, `IPasswordHasher`, etc.

### `Acme.SaaS.Infrastructure.Tests`
Tests infrastructure implementations with real or in-memory databases:
- GenericRepository CRUD operations
- Tenant isolation (Separate Schema vs Shared Schema)
- Interceptor behavior (Audit, SoftDelete, Tenant)
- UnitOfWork transaction handling
- Password hashing (BCrypt)
- FeatureGatingService plan checks

**Framework**: xUnit
**Database**: InMemory provider or Testcontainers for SQL Server

### `Acme.SaaS.API.Tests`
Tests the HTTP layer end-to-end:
- Controller endpoint behavior
- Middleware pipeline (exception handling, tenant resolution)
- Authentication and authorization
- Request/response serialization
- Swagger operation filters

**Framework**: xUnit + Microsoft.AspNetCore.TestHost
**Approach**: Integration tests with `WebApplicationFactory`
