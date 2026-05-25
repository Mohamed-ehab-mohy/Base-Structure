# Folder Directory Map

This document describes the responsibilities of each directory in the proposed multi-tenant SaaS base structure.

## Root

### `src/`

Contains all production source code. It is split by architectural layer to keep business logic independent from infrastructure and presentation concerns.

### `tests/`

Contains automated tests. Tests are organized by the same architecture boundaries used in `src/`.

### `docs/`

Contains architecture documentation, folder responsibilities, migration notes, and team decisions.

## Source Structure

### `src/Acme.SaaS.Domain/`

The core business layer. This layer must not depend on API, database, framework, or infrastructure implementations.

Responsibilities:

- define entities such as `Tenant`, `User`, `Plan`, and business-owned models
- define value objects and enums
- define domain exceptions
- keep business rules independent from external tools and frameworks

Suggested subdirectories:

- `Common/`: shared base entities, audit fields, domain primitives
- `Entities/`: core business entities
- `Enums/`: domain-specific enumerations
- `Exceptions/`: domain-level exceptions

### `src/Acme.SaaS.Application/`

The application/use-case layer. This layer coordinates business flows and defines contracts that infrastructure will implement.

Responsibilities:

- define service interfaces used by the application
- define DTOs, commands, queries, validators, and mapping profiles
- coordinate feature workflows through application services
- keep tenant-aware use cases separate from infrastructure details

Suggested subdirectories:

- `Common/Interfaces/`: contracts such as `ITenantProvider`, `ICurrentUserService`, and repository interfaces
- `Common/Behaviors/`: pipeline behaviors such as validation, logging, authorization, and feature gating
- `Common/DTOs/`: shared request and response models
- `MiniServices/`: feature-focused application services such as tenants, identity, billing, and products
- `Mapping/`: mapping profiles between entities and DTOs
- `Exceptions/`: application-level exceptions
- `Extensions/`: dependency injection helpers for the application layer

### `src/Acme.SaaS.Infrastructure/`

The implementation layer for persistence, multi-tenancy, caching, integrations, and external services.

Responsibilities:

- implement repositories and application interfaces
- provide tenant resolution and tenant connection services
- manage master and tenant databases
- contain migrations, database configurations, interceptors, caching, and integrations

Suggested subdirectories:

- `Persistence/Contexts/`: database contexts such as `MasterDbContext` and `ApplicationDbContext`
- `Persistence/Migrations/Master/`: migrations for global platform tables such as tenants and plans
- `Persistence/Migrations/Tenant/`: migrations for tenant-specific business data
- `Persistence/Repositories/`: repository implementations
- `Persistence/Configurations/`: database entity configurations
- `Persistence/Interceptors/`: tenant filters, audit fields, and save-change interceptors
- `MultiTenancy/Services/`: tenant provider and tenant connection implementations
- `MultiTenancy/Store/`: tenant lookup store backed by database, memory cache, or Redis
- `Services/Identity/`: authentication and identity infrastructure
- `Services/FeatureGating/`: plan and feature availability checks
- `Services/CustomLogic/`: strategies and factories for tenant-specific behavior
- `Extensions/`: dependency injection helpers for infrastructure services

### `src/Acme.SaaS.API/`

The presentation and HTTP entry-point layer.

Responsibilities:

- expose API endpoints
- resolve tenants from incoming requests
- configure middleware, filters, authentication, authorization, and dependency injection
- translate application results into HTTP responses

Suggested subdirectories:

- `Controllers/`: API endpoints grouped by feature
- `Middlewares/`: request pipeline middleware such as `TenantResolutionMiddleware`
- `Filters/`: API filters for validation, exception handling, and authorization
- `Resources/`: localization or API response resources
- `Extensions/`: API dependency injection and middleware registration

## Test Structure

### `tests/Acme.SaaS.Domain.Tests/`

Tests pure business rules, entities, value objects, and domain behavior.

### `tests/Acme.SaaS.Application.Tests/`

Tests use cases, validation, mapping, and application services using mocked infrastructure contracts.

### `tests/Acme.SaaS.Infrastructure.Tests/`

Tests database access, tenant resolution implementations, repositories, caching, and external service adapters.

### `tests/Acme.SaaS.API.Tests/`

Tests API behavior, middleware, request validation, authentication flows, and tenant resolution from HTTP requests.

## Documentation Structure

### `docs/folder-directory-map.md`

Explains every major directory and its responsibility.

### `docs/migration-changelog.md`

Explains what changed, why the structure supports SaaS, and how an existing project would be migrated.

