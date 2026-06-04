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
- **MVP velocity** — 4 projects only. New developers onboard in days, not weeks.
- **Technology-agnostic domain** — Switch from SQL Server to PostgreSQL, or REST to gRPC, without touching business rules.
- **Parallel work** — Frontend team works against API contracts while backend team implements infrastructure.

---

### 2. Tenant Context Isolation — Tenant-Agnostic Core

**Before:** Tenant ID passed manually through every service and controller method.

```
public async Task<IActionResult> GetProducts(Guid tenantId)
{
    return Ok(await _productService.GetAllAsync(tenantId));
}
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

**Why this benefits SaaS:**
- Domain and Application layers contain **zero tenant logic** — they operate in a single-tenant context unaware of multi-tenancy.
- Adding a new tenant resolution method (e.g., API key, custom domain) requires **zero changes** to business code.
- Security: tenant context flows through `HttpContext.Items`, not user-manipulable request parameters.

---

### 3. Mini-Services Pattern — Feature Isolation Without Overhead

**Before:** A single `Services/` folder with monolithic service classes containing mixed responsibilities.

```
Services/
├── ProductService.cs     // CRUD + pricing + inventory + shipping logic
├── UserService.cs        // Auth + profile + permissions + notifications
└── ReportService.cs      // All reports in one file
```

**After:** Each feature is a self-contained **Mini-Service** with its own Interface + Implementation.

```
MiniServices/
├── Tenants/       // ITenantService  + TenantService
├── Identity/      // IAuthService    + AuthService
├── Billing/       // IBillingService + BillingService
└── Products/      // IProductService + ProductService
```

**Why this benefits SaaS:**
- **No god-classes** — each service owns its domain logic explicitly.
- **CQRS-ready** — each Mini-Service can graduate to a full CQRS module without restructuring the entire application.
- **Feature gating** — applying plan-based restrictions (Free vs Pro vs Enterprise) at the service boundary is trivial.
- **Team scaling** — two developers can work on different Mini-Services simultaneously with zero merge conflicts.

---

### 4. Multi-Schema Database Strategy — Absolute Tenant Isolation

**Before:** Single database, single schema. All tenants share the same tables with a `TenantId` discriminator column.

```
app_db/
├── dbo.Products     (1M rows — all tenants mixed)
├── dbo.Orders       (500K rows — all tenants mixed)
└── dbo.Users        (200K rows — all tenants mixed)
```

**After:** Master/Tenant DbContext split with schema-per-tenant isolation.

```
master_db (shared — one instance)
└── dbo.Tenants      (Identifier, SchemaName, Plan, Status)

tenant_acme_db (per tenant — isolated)
└── tenant_acme.Products
└── tenant_acme.Orders
└── tenant_acme.Users

tenant_xyz_db (per tenant — isolated)
└── tenant_xyz.Products
└── tenant_xyz.Orders
└── tenant_xyz.Users
```

**Why this benefits SaaS:**
- **Absolute data isolation** — no `WHERE TenantId = @id` filter can leak data between tenants.
- **Backup per tenant** — restore a single tenant without impacting others.
- **Migrations are independent** — tenant schemas can be on different migration versions during rolling upgrades.
- **Performance** — tenant queries scan tables with only their data, no global index contention.

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

The following issues were identified during code review and corrected in subsequent commits:

| # | Issue | Fix | Commit |
|---|---|---|---|
| 1 | `TenantConnectionService` registered without its required `string` constructor parameter, causing a **runtime DI resolution failure**. | Registered with explicit factory passing `masterConnectionString`. | `7fa26d7` |
| 2 | `MappingProfile` was empty — **all AutoMapper-based API calls would throw** `AutoMapperMappingException`. | Added `CreateMap<Tenant, TenantDto>()` and `CreateMap<Product, ProductDto>()`. | `810197d` |
| 3 | `TenantInterceptor` hardcoded to only handle `Product` entities. Would miss `User` and future entities with `TenantId`. | Introduced `ITenantEntity` interface; interceptor now handles any implementing entity generically. | `06f0e41` |
| 4 | `SchemaService` depended on generic `DbContext` — **ambiguous at runtime** with two registered DbContexts (`MasterDbContext` + `ApplicationDbContext`). | Changed to explicit `MasterDbContext` dependency. | `ade8848` |
| 5 | Duplicate audit logic — `ApplicationDbContext.SaveChangesAsync` and `AuditableEntityInterceptor` both set `CreatedAt`/`UpdatedAt`. | Removed `SaveChangesAsync` override from `ApplicationDbContext`. Interceptors now own all audit fields exclusively. | `55ad461` |
| 6 | Password "hashing" was **reversible Base64 encoding** — a critical security vulnerability. | Replaced with BCrypt via `IPasswordHasher` interface + `PasswordHasher` implementation. `LoginAsync` now actually verifies password hash. | `e9ff606` |
| 7 | `BillingController.Upgrade` used `[FromBody] string plan` — poor Swagger documentation experience. | Replaced with typed `UpgradePlanRequest` DTO record. | `2ce1814` |
| 8 | Missing migration output directories. | Created `Persistence/Migrations/Master/` and `Persistence/Migrations/Tenant/` with `.gitkeep`. | `9262e9a` |

---

## Proposed Migration Plan (Existing Project → This Structure)

For an existing single-tenant project, the migration should follow this sequence:

### Step 1 — Extract Domain
Move entities, enums, value objects, and domain exceptions into `Domain` project.
- **Goal:** Zero dependencies on frameworks, databases, or external libraries.

### Step 2 — Define Application Contracts
Move use cases, DTOs, service interfaces, mapping profiles, and validation into `Application`.
- **Goal:** Infrastructure becomes a plug-in. Business logic is independently testable.

### Step 3 — Implement Infrastructure
Move database contexts, repositories, migrations, caching, email, file storage, and external integrations into `Infrastructure`.
- **Goal:** Every external concern has a home behind an Application-defined interface.

### Step 4 — Wire Presentation
Move controllers, middleware, filters, and startup configuration into `API`.
- **Goal:** The HTTP layer is thin — no business logic, only routing and orchestration.

### Step 5 — Introduce Multi-Tenancy
1. Add `ITenantProvider` to Application layer.
2. Implement `TenantResolutionMiddleware` in API layer.
3. Add `MasterDbContext` + `ApplicationDbContext` in Infrastructure.
4. Separate migrations into `Master/` and `Tenant/` directories.
5. Register `TenantStore` for in-memory/Redis caching of tenant lookups.

### Step 6 — Adopt Schema-Per-Tenant
1. Create tenant schema on registration via `SchemaService`.
2. Run tenant migrations against the new schema.
3. Verify isolation — Tenant A cannot access Tenant B's schema.

### Step 7 — Add Feature Gating
1. Map features to minimum `SubscriptionPlan` levels.
2. Apply `IFeatureGatingService` at service boundaries.
3. Return 403 with clear messaging when a feature is not available.

### Step 8 — Tests
Add layer-specific test projects covering:
- **Domain:** Entity invariants, value object equality, domain exception behavior.
- **Application:** Mini-Service orchestration with mocked `ITenantProvider` and `IApplicationDbContext`.
- **Infrastructure:** Tenant resolution, repository filtering, interceptor behavior.
- **API:** Middleware pipeline, tenant resolution from subdomain/header/JWT.

---

## Filesystem Delta Summary

```
Created:
├── src/Acme.SaaS.Domain/              (6 files)
├── src/Acme.SaaS.Application/         (18 files)
├── src/Acme.SaaS.Infrastructure/      (20 files)
├── src/Acme.SaaS.API/                 (16 files)
├── tests/                             (4 test projects)
└── docs/                              (2 documentation files)

Fixed Post-Review:
├── 7 bug fixes across all layers
├── 1 dependency injection correction
├── 1 security hardening (BCrypt)
├── 3 code quality improvements
└── 2 documentation updates
```

---

## Compatibility Notes

| Concern | Decision |
|---|---|
| **.NET Version** | 10.0 — latest stable, LTS-ready |
| **ORM** | Entity Framework Core 10.0 |
| **Database** | SQL Server (LocalDB for dev) |
| **Auth** | JWT Bearer tokens |
| **Password Hashing** | BCrypt (via BCrypt.Net-Next) |
| **Mapping** | AutoMapper |
| **Tenant Isolation** | Schema-per-tenant (separate schemas, same database instance) |
| **Cache** | In-memory `ConcurrentDictionary` (swappable to Redis) |
| **API Documentation** | Swagger / OpenAPI |
| **Testing** | xUnit |
