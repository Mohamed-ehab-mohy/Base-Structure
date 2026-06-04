# Acme SaaS — Multi-Tenant Base Structure

A production-ready **multi-tenant SaaS** template built with **.NET 10** following **Clean Architecture** principles. Designed for medium-sized SaaS applications with tenant isolation, feature gating, and per-tenant customization.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Acme.SaaS.sln                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  📦 Domain (Core)            // Zero external dependencies                  │
│  ├── Common/                 // BaseEntity, BaseAuditableEntity, ValueObject│
│  ├── Entities/               // Tenant, User, Product, Role                │
│  ├── Enums/                  // SubscriptionPlan, TenantStatus, UserRole    │
│  └── Exceptions/             // DomainException, NotFoundException         │
│                                                                             │
│  📦 Application              // Business logic + interfaces                 │
│  ├── Common/                                                               │
│  │   ├── Behaviors/          // LoggingBehavior, ValidationBehavior        │
│  │   ├── DTOs/               // ApiResponse<T>, PagedResult<T>             │
│  │   ├── Exceptions/         // ValidationException, ForbiddenException    │
│  │   └── Interfaces/         // ITenantProvider, ICurrentUserService,      │
│  │                              IGenericRepository<T>, IUnitOfWork,         │
│  │                              IApplicationDbContext, IPasswordHasher       │
│  ├── Mapping/                // MappingProfile (AutoMapper)                 │
│  ├── MiniServices/           // ⭐ Feature modules                          │
│  │   ├── Tenants/            // ITenantService + TenantService             │
│  │   ├── Identity/           // IAuthService + AuthService                 │
│  │   ├── Billing/            // IBillingService + BillingService           │
│  │   └── Products/           // IProductService + ProductService           │
│  └── Extensions/             // DI registration                            │
│                                                                             │
│  📦 Infrastructure           // Implementations                             │
│  ├── MultiTenancy/           // ⭐ Tenant isolation core                    │
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
│  ├── Middlewares/            // ⭐ TenantResolutionMiddleware,              │
│  │                              ExceptionHandlingMiddleware,                │
│  │                              RequestLoggingMiddleware                    │
│  ├── Extensions/             // DI + pipeline configuration                │
│  └── Filters/                // SwaggerDefaultValues                       │
│                                                                             │
│  📦 Tests                                                                   │
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

## Architecture Decisions

### Clean Architecture (4 Projects)

| Layer | Responsibility | Dependencies |
|---|---|---|
| **Domain** | Entities, enums, exceptions, base classes | None |
| **Application** | Use cases, DTOs, interfaces, mapping | Domain |
| **Infrastructure** | Persistence, multi-tenancy, identity, services | Application |
| **API** | Controllers, middleware, DI setup | Infrastructure |

### Multi-Tenancy Strategy

**Tenant Resolution** (in `TenantResolutionMiddleware`):
1. **Subdomain** — `tenant1.acme.com` → extracts `tenant1`
2. **Header** — `X-Tenant-Id` HTTP header
3. **JWT Claim** — `TenantId` claim from token

**Isolation Model**: **Schema-per-Tenant**
- Each tenant gets a dedicated database schema (`tenant_{identifier}`)
- `ApplicationDbContext` switches schema at runtime via `HasDefaultSchema()`
- No `TenantId` column needed on business tables — isolation is schema-native
- `MasterDbContext` holds global data (tenants, plans) in a shared schema

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
- **PII, secrets, and connection strings** are never hardcoded; they come from configuration, environment variables, or a vault

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
| `Domain/Common/` | Shared kernel — base entities, value objects |
| `Domain/Entities/` | Core business entities |
| `Application/Common/Interfaces/` | Contracts implemented by infrastructure |
| `Application/MiniServices/` | Feature modules (one folder per feature) |
| `Infrastructure/MultiTenancy/` | Tenant resolution, schema, connection management |
| `Infrastructure/Persistence/` | EF Core contexts, migrations, configurations |
| `Infrastructure/Services/Identity/` | JWT, current user, password hashing |
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
  │
  ▼
Authentication (JWT) → Authorization
  │
  ▼
Controller → MiniService → DbContext  // Schema-isolated data access
```

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
