# Migration Changelog

## Context

This repository provides a **SaaS-ready base structure** built from scratch as a starting template for migrating an unstructured or single-tenant project into a multi-tenant SaaS architecture.

The changelog below documents every architectural shift, structural change, and post-initial fix applied to ensure the template is production-viable from day one.

---

## Architectural Shifts (Migration from Single-Tenant)

### 1. Horizontal Layer Slicing — Standard Clean Architecture

**Before (Typical Single-Tenant Monolith):**
```
📦 Monolith.Web
├── Models/
├── Views/
├── Controllers/
├── Services/
├── Data/
└── Migrations/
```

**After:**
```
📦 Acme.SaaS.sln
├── 01. Acme.SaaS.Domain          // Zero dependencies
├── 02. Acme.SaaS.Application      // Depends on Domain only
├── 03. Acme.SaaS.Infrastructure   // Implements Application contracts
└── 04. Acme.SaaS.API              // Presentation entry point
```

**Why this benefits SaaS:**
- **MVP velocity** — 4 projects only. New developers onboard in days.
- **Technology-agnostic domain** — Switch from SQL Server to PostgreSQL, or REST to gRPC, without touching business rules.
- **Parallel work** — Frontend team works against API contracts while backend implements infrastructure.

---

### 2. Tenant Context Isolation — Tenant-Agnostic Core

**Before:** Tenant ID passed manually through every service and controller method.

```csharp
public async Task<IActionResult> GetProducts(Guid tenantId) { ... }
```

**After:** Tenant detection abstracted behind `ITenantProvider` in the Application layer, implemented in Infrastructure via `TenantResolutionMiddleware`.

```csharp
// Controller — tenant-agnostic
public async Task<IActionResult> GetProducts()
{
    return Ok(await _productService.GetAllAsync());
}

// Service — resolves tenant transparently
public async Task<List<Product>> GetAllAsync()
{
    var tenantId = _tenantProvider.GetTenantId(); // 👈 No parameter
    return await _context.Products
        .Where(p => p.TenantId == tenantId)
        .ToListAsync();
}
```

**Resolution pipeline (in order):**
1. Subdomain — `tenant1.acme.com`
2. HTTP Header — `X-Tenant-Id`
3. JWT Claim — `TenantId`

---

### 3. Mini-Services Pattern — Feature Isolation Without Overhead

**Before:** A single `Services/` folder with monolithic service classes.

**After:** Each feature is a self-contained **Mini-Service** with its own Interface + Implementation.

```
MiniServices/
├── Tenants/       // ITenantService  + TenantService
├── Identity/      // IAuthService    + AuthService
├── Billing/       // IBillingService + BillingService
└── Products/      // IProductService + ProductService
```

---

### 4. Schema Isolation Strategy

**Before:** Single database, single schema. All tenants share the same tables with a `TenantId` discriminator column.

**After:** Master/Tenant DbContext split with mode-aware isolation — configurable via `TenancyOptions.Mode`.

```
master_db (shared — one instance)
└── dbo.Tenants      (Identifier, SchemaName, Plan, Status)

app_db (shared database)
├── [Separate Schema] tenant_1.Products, tenant_2.Products  // schema isolation
└── [Shared Schema]  dbo.Products (TenantId discriminator)   // filter isolation
```

---

## Decision Record: Separate Schema vs Shared Schema

| Criterion | Shared Schema | Separate Schema ✅ |
|---|---|---|
| **Data isolation** | Logical — `WHERE TenantId` filter | Physical — separate schema namespace |
| **Data leak risk** | One missing `WHERE` leaks all tenants | Impossible — wrong schema is a catalog error |
| **Backup & restore** | All tenants together | Per-tenant (selective restore) |
| **Migration independence** | Single migration for all tenants | Each schema can migrate independently |
| **Query performance** | Index contention at scale | Schema-local indexes |
| **Onboarding complexity** | Minimal — just insert a row | Medium — create schema + run migrations |
| **Upgrade path to dedicated DB** | Row-level data extraction | Trivial — `CREATE DATABASE` + copy schema |

### Why Separate Schema Is the Right Choice

**1. Defense in depth** — physical isolation beats logical isolation. A missing `WHERE TenantId` is structurally impossible.

**2. Enterprise-ready** — Graduates to per-tenant databases with zero code changes.

**3. Safer migrations** — Roll out to one tenant for testing before global apply.

**4. Audit & compliance** — Schema is the isolation boundary; no need to hope filters are correct.

**5. MVP philosophy** — Simpler mental model: "I operate in my tenant's schema, I don't need to think about filtering."

---

## Implementation Status

Both modes are configurable via `appsettings.json`:

```json
{
  "TenancyOptions": {
    "Mode": "SeparateSchema"   // or "SharedSchema"
  }
}
```

### Mode-dependent behavior

| Behavior | Separate Schema | Shared Schema |
|---|---|---|
| `GenericRepository` queries | No tenant filter — schema isolates | Auto-appends `WHERE TenantId = @id` |
| `TenantInterceptor` on `Added` | Sets `TenantId` (audit-friendly) | Sets `TenantId` (required) |
| `TenantInterceptor` on `Modified` | Allows changes | Blocks `TenantId` changes |
| Business tables need `TenantId` | Optional (audit only) | Required |
| Schema isolation | `HasDefaultSchema(schemaName)` | Single shared schema |
| Connection string | Same master DB (schema isolates) | Same master DB (filter isolates) |

The connection string is always the master database — isolation is handled by schema (`HasDefaultSchema`) or by automatic `TenantId` filtering depending on the mode.

---

## Changes Applied to This Repository

### Phase 1 — Initial Structure (Baseline)

| Change | Files | Purpose |
|---|---|---|
| Created Clean Architecture projects | 4 `.csproj` files | Horizontal layer slicing |
| Added Domain entities | `Tenant`, `User`, `Product`, `Role` | Core business model |
| Added Domain shared kernel | `BaseEntity`, `BaseAuditableEntity`, `ValueObject` | Consistency across entities |
| Added Domain enums | `SubscriptionPlan`, `TenantStatus`, `UserRole` | Strongly-typed domain values |
| Added Application interfaces | `ITenantProvider`, `ICurrentUserService`, `IGenericRepository<T>`, `IUnitOfWork`, `IApplicationDbContext` | Dependency inversion contracts |
| Added Application DTOs | `ApiResponse<T>`, `PagedResult<T>` | Unified API response shape |
| Added Mini-Services | Tenants, Identity, Billing, Products | Feature isolation |
| Added MultiTenancy layer | `TenantProvider`, `SchemaService`, `TenantConnectionService`, `TenantStore` | Centralized tenant management |
| Added Persistence layer | `MasterDbContext`, `ApplicationDbContext`, `GenericRepository<T>`, `UnitOfWork` | Data access with tenant isolation |
| Added EF Core interceptors | `AuditableEntityInterceptor`, `SoftDeleteInterceptor`, `TenantInterceptor` | Cross-cutting data concerns |
| Added Entity configurations | `TenantConfiguration`, `UserConfiguration`, `ProductConfiguration` | Schema mapping |
| Added Identity services | `JwtTokenService`, `CurrentUserService` | Authentication plumbing |
| Added Feature gating | `FeatureGatingService` | Plan-based access control |
| Added Strategy pattern | `StandardTaxStrategy`, `VodafoneTaxStrategy`, `TaxStrategyFactory` | Per-tenant business logic |
| Added API middleware | `TenantResolutionMiddleware`, `ExceptionHandlingMiddleware`, `RequestLoggingMiddleware` | Request pipeline |
| Added Swagger filter | `SwaggerDefaultValues` | API documentation |
| Added DI extensions | Per-layer `ServiceCollectionExtensions` | Clean startup configuration |

### Phase 2 — Post-Initial Fixes (Code Review)

| # | Issue | Fix | Commit |
|---|---|---|---|
| 1 | `TenantConnectionService` registered without `string` constructor parameter — runtime DI failure. | Registered with explicit factory passing `masterConnectionString`. | `7fa26d7` |
| 2 | `MappingProfile` was empty — AutoMapper calls would throw. | Added `CreateMap<Tenant, TenantDto>()` and `CreateMap<Product, ProductDto>()`. | `810197d` |
| 3 | `TenantInterceptor` hardcoded to `Product` only — missed `User` and future entities. | Introduced `ITenantEntity` interface; interceptor handles any implementing entity generically. | `06f0e41` |
| 4 | `SchemaService` depended on generic `DbContext` — ambiguous with two registered DbContexts. | Changed to explicit `MasterDbContext` dependency. | `ade8848` |
| 5 | Duplicate audit logic — `ApplicationDbContext.SaveChangesAsync` + interceptor both set audit fields. | Removed `SaveChangesAsync` override from `ApplicationDbContext`. | `55ad461` |
| 6 | Password "hashing" was reversible Base64 — critical security vulnerability. | Replaced with BCrypt via `IPasswordHasher` + `PasswordHasher`. `LoginAsync` now verifies hash. | `e9ff606` |
| 7 | `BillingController.Upgrade` used `[FromBody] string` — poor Swagger experience. | Replaced with typed `UpgradePlanRequest` DTO. | `2ce1814` |
| 8 | Missing migration output directories. | Created `Migrations/Master/` and `Migrations/Tenant/` with `.gitkeep`. | `9262e9a` |
| 9 | `TenantConnectionService` modified connection string with schema suffix — created non-existent database. | Removed `TenantConnectionService` from `ApplicationDbContext` factory; uses `masterConnectionString` directly. Schema isolation handled by `HasDefaultSchema()`. | Current |
| 10 | Config switch (`TenancyOptions.Mode`) existed in design but not wired to code. | Added `TenancyOptions` class, registered from config, used in `GenericRepository` and `TenantInterceptor` for mode-aware behavior. | Current |

---

## Proposed Migration Plan (Existing Project → This Structure)

### Step 1 — Extract Domain
Move entities, enums, value objects, and domain exceptions into `Domain` project.

### Step 2 — Define Application Contracts
Move use cases, DTOs, service interfaces, mapping profiles, and validation into `Application`.

### Step 3 — Implement Infrastructure
Move database contexts, repositories, migrations, caching, email, file storage, and external integrations into `Infrastructure`.

### Step 4 — Wire Presentation
Move controllers, middleware, filters, and startup configuration into `API`.

### Step 5 — Introduce Multi-Tenancy
1. Add `ITenantProvider` to Application layer.
2. Implement `TenantResolutionMiddleware` in API layer.
3. Add `MasterDbContext` + `ApplicationDbContext` in Infrastructure.
4. Separate migrations into `Master/` and `Tenant/` directories.
5. Register `TenantStore` for in-memory/Redis caching of tenant lookups.

### Step 6 — Adopt Schema Isolation
1. Set `TenancyOptions.Mode` in `appsettings.json`.
2. In Separate Schema mode: `SchemaService` creates schema on registration; migrations run per-schema.
3. In Shared Schema mode: `GenericRepository` auto-filters by `TenantId`; entities must implement `ITenantEntity`.

### Step 7 — Add Feature Gating
1. Map features to minimum `SubscriptionPlan` levels.
2. Apply `IFeatureGatingService` at service boundaries.
3. Return 403 with clear messaging when a feature is not available.

### Step 8 — Tests
Add layer-specific test projects covering Domain, Application (mocked), Infrastructure (repository filtering, interceptors), and API (middleware pipeline).

---

## Compatibility Notes

| Concern | Decision |
|---|---|
| **.NET Version** | 10.0 |
| **ORM** | Entity Framework Core 10.0 |
| **Database** | SQL Server (LocalDB for dev) |
| **Auth** | JWT Bearer tokens |
| **Password Hashing** | BCrypt (BCrypt.Net-Next) |
| **Mapping** | AutoMapper |
| **Tenant Isolation** | Configurable: Separate Schema (default) or Shared Schema |
| **Cache** | In-memory `ConcurrentDictionary` (swappable to Redis) |
| **API Documentation** | Swagger / OpenAPI |
| **Testing** | xUnit |
