# Acme SaaS — Multi-Tenant Base Structure

A production-ready **multi-tenant SaaS** template built with **.NET 10** following **Clean Architecture** principles. Designed for medium-sized SaaS applications with tenant isolation, feature gating, and per-tenant customization.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Acme.SaaS.sln                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  📦 Domain (Core)            // Zero external dependencies                  │
│  ├── Common/                 // BaseEntity, BaseAuditableEntity, ValueObject│
│  │                              ITenantEntity                               │
│  ├── Entities/               // Tenant, User, Product, Role                │
│  ├── Enums/                  // SubscriptionPlan, TenantStatus, UserRole    │
│  └── Exceptions/             // DomainException, NotFoundException         │
│                                                                             │
│  📦 Application              // Business logic + interfaces                 │
│  ├── Common/                                                               │
│  │   ├── Behaviors/          // LoggingBehavior, ValidationBehavior        │
│  │   ├── DTOs/               // ApiResponse<T>, PagedResult<T>             │
│  │   ├── Exceptions/         // ValidationException, ForbiddenException    │
│  │   ├── Mapping/            // MappingProfile (AutoMapper)                 │
│  │   └── Interfaces/         // ITenantProvider, ICurrentUserService,      │
│  │                              IGenericRepository<T>, IUnitOfWork,         │
│  │                              IApplicationDbContext, IPasswordHasher       │
│  ├── MiniServices/           // ⭐ Feature modules                          │
│  │   ├── Tenants/            // ITenantService + TenantService             │
│  │   ├── Identity/           // IAuthService + AuthService                 │
│  │   ├── Billing/            // IBillingService + BillingService           │
│  │   └── Products/           // IProductService + ProductService           │
│  └── Extensions/             // DI registration                            │
│                                                                             │
│  📦 Infrastructure           // Implementations                             │
│  ├── MultiTenancy/           // ⭐ Tenant isolation core                    │
│  │   ├── TenancyOptions.cs   // Mode: SeparateSchema / SharedSchema        │
│  │   ├── Services/           // TenantProvider, SchemaService,             │
│  │   │                          TenantConnectionService                     │
│  │   └── Store/              // TenantStore (in-memory cache)               │
│  ├── Persistence/                                                          │
│  │   ├── Contexts/           // MasterDbContext, ApplicationDbContext       │
│  │   ├── Migrations/                                                        │
│  │   │   ├── Master/         // Global tables (Tenants, Plans)             │
│  │   │   └── Tenant/         // Per-tenant business tables                 │
│  │   ├── Repositories/       // GenericRepository<T>, UnitOfWork           │
│  │   ├── Configurations/     // Entity type configurations                 │
│  │   └── Interceptors/       // AuditableEntityInterceptor,                │
│  │                              SoftDeleteInterceptor, TenantInterceptor    │
│  └── Services/                                                              │
│      ├── Identity/           // JwtTokenService, CurrentUserService,       │
│      │                          PasswordHasher (BCrypt)                     │
│      ├── FeatureGating/      // FeatureGatingService                       │
│      └── CustomLogic/        // Strategy pattern per-tenant                │
│          ├── Strategies/     // StandardTaxStrategy, VodafoneTaxStrategy   │
│          └── Factories/      // TaxStrategyFactory                         │
│                                                                             │
│  📦 API (Presentation)       // HTTP entry point                            │
│  ├── Controllers/            // TenantsController, AuthController,         │
│  │                              ProductsController, BillingController       │
│  ├── Middlewares/                                                           │
│  │   ├── ⭐ TenantResolutionMiddleware  // Subdomain/Header/JWT resolution  │
│  │   ├── ExceptionHandlingMiddleware    // Global exception → JSON          │
│  │   └── RequestLoggingMiddleware       // Method, path, status, duration   │
│  ├── Extensions/             // DI + pipeline configuration                │
│  └── Filters/                // SwaggerDefaultValues                       │
│                                                                             │
│  📦 Tests (xUnit)                                                           │
│  ├── Domain.Tests                                                          │
│  ├── Application.Tests                                                     │
│  ├── Infrastructure.Tests                                                  │
│  └── API.Tests                                                             │
│                                                                             │
├─────────────────────────────────────────────────────────────────────────────┤
│  Runtime: .NET 10  |  DB: SQL Server  |  Auth: JWT Bearer                   │
│  Cache: In-Memory  |  Map: AutoMapper  |  Hash: BCrypt                      │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🏢 Folder Directory Map

Our architecture follows the **Standard Clean Architecture (Horizontal Slicing)** split into 4 core projects to maximize MVP velocity and eliminate over-engineering:

1. **01. Domain:** Contains Enterprise Entities (Tenant, User, Product), Enums, and Core Exceptions. Fully isolated with zero dependencies.

2. **02. Application:** Holds the core Business Logic wrapped in highly encapsulated **Mini-Services** (Identity, Tenants, Billing, Products) instead of full CQRS/MediatR overhead.

3. **03. Infrastructure:** Handles Data Access (ApplicationDbContext vs MasterDbContext) and Multi-Tenancy strategies (Per-tenant connection strings & Schema isolation).

4. **04. Presentation.API:** The application entry point containing Controllers, Middlewares (TenantResolution), and configurations.

## Architecture Decisions

### Multi-Tenancy Strategy

**Tenant Resolution** (in `TenantResolutionMiddleware`):
1. **Subdomain** — `tenant1.acme.com` → extracts `tenant1`
2. **Header** — `X-Tenant-Id` HTTP header
3. **JWT Claim** — `TenantId` claim from token

**Isolation Model**: Configurable via `appsettings.json`:

```json
{
  "TenancyOptions": {
    "Mode": "SeparateSchema"
  }
}
```

| Feature | Separate Schema (default) | Shared Schema |
|---|---|---|
| Approach | Schema-per-tenant (`HasDefaultSchema`) | Filter-per-tenant (`WHERE TenantId`) |
| `GenericRepository` | No filter — schema isolates | Auto `WHERE TenantId = @id` |
| `TenantInterceptor` | Sets `TenantId` on add | Sets + blocks changes on modified |
| `ITenantEntity` | Optional (audit) | Required |
| `TenantId` on entities | Optional | Required |

**Connection String**: Always the master database. Isolation is handled by schema (Separate Schema) or by automatic row filtering (Shared Schema). `TenantConnectionService` is available for database-per-tenant scenarios but not used in the current schema-per-tenant mode.

## 🔄 Why this Structure benefits our SaaS Implementation?

* **Strict Tenant Isolation:** Tenant detection is completely handled at the Infrastructure/Middleware level. Domain and Application layers are completely tenant-agnostic.

* **Database Strategy:** Implemented a Master/Tenant DB split. Master DB stores tenant metadata, while business data is logically isolated via per-tenant schemas.

* **Runtime Reliability:** Connection strings are resolved dynamically per-request using `TenantConnectionService` wired directly into the context factory.

### Mini-Services Pattern

Each feature is a self-contained module with its own interface + implementation. This avoids CQRS over-engineering while maintaining separation of concerns.

### Per-Tenant Customization

Strategy + Factory pattern for business logic that varies by tenant:
- **`ITaxCalculationStrategy`** — `StandardTaxStrategy` (10%) vs `VodafoneTaxStrategy` (14% + 5)
- **`TaxStrategyFactory`** — selects the right strategy per tenant

### Feature Gating

`FeatureGatingService` controls feature access based on the tenant's `SubscriptionPlan`:
- `Free` → basic CRUD only
- `Pro` → export, API access
- `Enterprise` → audit logs, custom branding

### Security

- Passwords hashed with **BCrypt** via `IPasswordHasher` / `PasswordHasher`
- **JWT** tokens with tenant and role claims
- **PII, secrets, and connection strings** are never hardcoded

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (LocalDB, Docker, or remote)

### Setup

```bash
git clone https://github.com/Mohamed-ehab-mohy/Base-Structure.git
cd Base-Structure

# Update connection string in appsettings.json if needed
# MasterDb: Server=(localdb)\mssqllocaldb;Database=AcmeSaaS_Master;...

dotnet restore
dotnet build
dotnet run --project src/Acme.SaaS.API
```

### EF Core Migrations

```bash
# Master database migrations (run once)
dotnet ef migrations add InitialMaster --output-dir Persistence/Migrations/Master
dotnet ef database update

# Tenant migrations (run for each new tenant schema)
dotnet ef migrations add InitialTenant --output-dir Persistence/Migrations/Tenant
```

---

## Project Structure Reference

| Path | Description |
|---|---|
| `Domain/Common/` | Shared kernel — base entities, value objects, `ITenantEntity` |
| `Domain/Entities/` | Core business entities |
| `Application/Common/Interfaces/` | Contracts implemented by infrastructure |
| `Application/MiniServices/` | Feature modules (one folder per feature) |
| `Infrastructure/MultiTenancy/` | `TenancyOptions`, tenant resolution, schema, connection |
| `Infrastructure/Persistence/` | EF Core contexts, migrations, configurations, interceptors |
| `Infrastructure/Services/Identity/` | JWT, current user, password hashing (BCrypt) |
| `Infrastructure/Services/CustomLogic/` | Per-tenant strategy pattern |
| `API/Middlewares/` | Request pipeline — tenant resolution, logging, exceptions |
| `API/Controllers/` | API endpoints |

---

## Pipeline Flow

```
HTTP Request
  │
  ▼
ExceptionHandlingMiddleware    // Global exception → JSON response
  │
  ▼
RequestLoggingMiddleware        // Method, path, status, duration
  │
  ▼
TenantResolutionMiddleware      // ⭐ Resolve tenant → HttpContext.Items
  │                             //    (TenantId, SchemaName, Plan, Status)
  ▼
Authentication (JWT) → Authorization
  │
  ▼
Controller → MiniService
  │         → GenericRepository (mode-aware filtering)
  │         → ApplicationDbContext (schema-isolated / filter-isolated)
```

---

## Documentation

- `folder-directory-map.md` — directory responsibilities
- `migration-changelog.md` — architectural shifts, bug fixes, decision records
- `folder_structure.html` — interactive visual map (Arabic/English)

---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'feat: add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## License

This project is provided as a starting template for multi-tenant SaaS applications.
