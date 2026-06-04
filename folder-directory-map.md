# Folder Directory Map

This document describes the responsibilities of each directory in the multi-tenant SaaS base structure.

## Root

### `src/`

Contains all production source code. It is split by architectural layer to keep business logic independent from infrastructure and presentation concerns.

### `tests/`

Contains automated tests. Tests are organized by the same architecture boundaries used in `src/`.

### `docs/` (optional)

Architecture diagrams, migration references, or design documents. Markdown documentation lives at the repo root.

## Source Structure

### `src/Acme.SaaS.Domain/`

The core business layer. This layer must not depend on API, database, framework, or infrastructure implementations.

Responsibilities:
- Define entities: `Tenant`, `User`, `Product`, `Role`
- Define value objects and enums
- Define domain exceptions
- Keep business rules independent from external tools

Subdirectories:
- `Common/`: `BaseEntity`, `BaseAuditableEntity`, `ValueObject`, `ITenantEntity`
- `Entities/`: core business entities
- `Enums/`: domain-specific enumerations
- `Exceptions/`: domain-level exceptions

### `src/Acme.SaaS.Application/`

The application/use-case layer. Coordinates business flows and defines contracts that infrastructure implements.

Responsibilities:
- Define service interfaces used by the application
- Define DTOs, mapping profiles, and validators
- Coordinate feature workflows through Mini-Services
- Keep tenant-aware use cases separate from infrastructure details

Subdirectories:
- `Common/Interfaces/`: `ITenantProvider`, `ICurrentUserService`, `IGenericRepository<T>`, `IUnitOfWork`, `IApplicationDbContext`, `IPasswordHasher`
- `Common/Behaviors/`: pipeline behaviors — `LoggingBehavior`, `ValidationBehavior`
- `Common/DTOs/`: shared request/response models — `ApiResponse<T>`, `PagedResult<T>`
- `Common/Exceptions/`: `ValidationException`, `ForbiddenException`
- `Common/Mapping/`: `MappingProfile` (AutoMapper)
- `MiniServices/`: feature-focused application services — Tenants, Identity, Billing, Products
- `Extensions/`: DI helpers for the application layer

### `src/Acme.SaaS.Infrastructure/`

The implementation layer for persistence, multi-tenancy, caching, integrations, and external services.

Responsibilities:
- Implement repositories and application interfaces
- Provide tenant resolution and connection services
- Manage master and tenant databases
- Contain migrations, configurations, interceptors, feature gating, and strategy pattern

Subdirectories:
- `MultiTenancy/`: tenant isolation core
  - `Services/`: `TenantProvider`, `SchemaService`, `TenantConnectionService`
  - `Store/`: `TenantStore` (in-memory/Redis cache)
- `Persistence/`: EF Core data access
  - `Contexts/`: `MasterDbContext`, `ApplicationDbContext`
  - `Migrations/Master/`: global platform tables (Tenants, Plans)
  - `Migrations/Tenant/`: per-tenant business tables
  - `Repositories/`: `GenericRepository<T>`, `UnitOfWork`
  - `Configurations/`: `TenantConfiguration`, `UserConfiguration`, `ProductConfiguration`
  - `Interceptors/`: `AuditableEntityInterceptor`, `SoftDeleteInterceptor`, `TenantInterceptor`
- `Services/`: infrastructure services
  - `Identity/`: `JwtTokenService`, `CurrentUserService`, `PasswordHasher`
  - `FeatureGating/`: `FeatureGatingService`
  - `CustomLogic/Strategies/`: `StandardTaxStrategy`, `VodafoneTaxStrategy`
  - `CustomLogic/Factories/`: `TaxStrategyFactory`
- `Extensions/`: DI helpers for infrastructure services

### `src/Acme.SaaS.API/`

The presentation and HTTP entry-point layer.

Responsibilities:
- Expose API endpoints
- Resolve tenants from incoming requests
- Configure middleware, filters, authentication, authorization, and DI
- Translate application results into HTTP responses

Subdirectories:
- `Controllers/`: `TenantsController`, `AuthController`, `ProductsController`, `BillingController`
- `Middlewares/`: `TenantResolutionMiddleware`, `ExceptionHandlingMiddleware`, `RequestLoggingMiddleware`
- `Extensions/`: `ServiceCollectionExtensions`, `ApplicationBuilderExtensions`
- `Filters/`: `SwaggerDefaultValues`

## Test Structure

### `tests/Acme.SaaS.Domain.Tests/`
Tests pure business rules, entities, value objects, and domain behavior.

### `tests/Acme.SaaS.Application.Tests/`
Tests use cases, validation, mapping, and application services using mocked infrastructure contracts.

### `tests/Acme.SaaS.Infrastructure.Tests/`
Tests database access, tenant resolution, repositories, caching, and external service adapters.

### `tests/Acme.SaaS.API.Tests/`
Tests API behavior, middleware, request validation, authentication flows, and tenant resolution from HTTP requests.
