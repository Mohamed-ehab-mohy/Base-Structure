# Folder Directory Map

This document describes the responsibilities of each directory and file in the multi-tenant SaaS base structure.

## Solution Structure

```
Acme.SaaS.slnx
├── src/
│   ├── Acme.SaaS.Domain/           # Core business entities and rules
│   ├── Acme.SaaS.Application/      # Business use cases and interfaces
│   ├── Acme.SaaS.Infrastructure/   # Data access, multi-tenancy, services
│   └── Acme.SaaS.API/              # HTTP entry point (Controllers, Middlewares)
├── tests/
│   ├── Acme.SaaS.Domain.Tests/
│   ├── Acme.SaaS.Application.Tests/
│   ├── Acme.SaaS.Infrastructure.Tests/
│   └── Acme.SaaS.API.Tests/
├── Directory.Build.props           # Common MSBuild properties
├── Acme.SaaS.slnx                  # Solution file (new .slnx format)
├── .gitignore
├── README.md                       # Project overview
├── folder-directory-map.md         # This file — directory responsibilities
├── folder_structure.html           # Interactive visual map (Arabic/English)
└── migration-changelog.md          # Architecture decisions and changelog
```

---

## Source Structure (`src/`)

### `src/Acme.SaaS.Domain/`

The core business layer with **zero external dependencies**. Contains enterprise entities, value objects, enums, and domain exceptions.

#### File-by-File

| File | Description |
|---|---|
| `Common/BaseEntity.cs` | Base class for all entities: `Id (Guid)`, `CreatedAt`, `CreatedBy` |
| `Common/BaseAuditableEntity.cs` | Extends BaseEntity: `UpdatedAt`, `UpdatedBy`, `IsDeleted` |
| `Common/ValueObject.cs` | DDD Value Object base with structural equality |
| `Common/ITenantEntity.cs` | Marks entities as tenant-scoped (required for Shared Schema) |
| `Entities/Tenant.cs` | SaaS tenant: Identifier, SchemaName, ConnectionString, Plan, Status |
| `Entities/User.cs` | Application user: Email, PasswordHash, Role, TenantId |
| `Entities/Role.cs` | Role definition: Name, Permissions list |
| `Entities/Product.cs` | Sample business entity (tenant-scoped) |
| `Enums/TenantStatus.cs` | Trial, Active, Suspended, Expired |
| `Enums/SubscriptionPlan.cs` | Free, Pro, Enterprise |
| `Enums/UserRole.cs` | SuperAdmin, TenantAdmin, Member |
| `Exceptions/DomainException.cs` | Base domain exception → 400 Bad Request |
| `Exceptions/NotFoundException.cs` | Entity not found → 404 Not Found |

---

### `src/Acme.SaaS.Application/`

The use-case layer. Contains business logic organized as Mini-Services, defines contracts (interfaces), DTOs, mapping profiles, and validation behaviors.

#### File-by-File

| File | Description |
|---|---|
| `Common/Interfaces/IApplicationDbContext.cs` | EF Core DbContext abstraction (Tenants, Users, Products, Roles) |
| `Common/Interfaces/ITenantProvider.cs` | Current tenant info: Id, Schema, Plan, Identifier |
| `Common/Interfaces/ICurrentUserService.cs` | Current user info: Id, Email, Role, IsAuthenticated |
| `Common/Interfaces/IUnitOfWork.cs` | Transaction management: Begin, Commit, Rollback |
| `Common/Interfaces/IGenericRepository.cs` | Generic CRUD: GetById, GetAll, Find, Any, Add, Update, Delete |
| `Common/Interfaces/IPasswordHasher.cs` | BCrypt password hashing abstraction |
| `Common/Behaviors/LoggingBehavior.cs` | Wraps operations with structured logging |
| `Common/Behaviors/ValidationBehavior.cs` | FluentValidation pipeline (future-ready) |
| `Common/DTOs/ApiResponse.cs` | Unified response: `{ Success, Data, Message, Errors }` |
| `Common/DTOs/PagedResult.cs` | Pagination: `{ Items, TotalCount, Page, Size, TotalPages }` |
| `Common/Exceptions/ValidationException.cs` | Validation errors → 400 with error list |
| `Common/Exceptions/ForbiddenException.cs` | Permission denied → 403 |
| `Common/Mapping/MappingProfile.cs` | AutoMapper: Entity ↔ DTO mappings |
| `MiniServices/Tenants/ITenantService.cs` | Tenant management service contract |
| `MiniServices/Tenants/TenantService.cs` | Tenant CRUD implementation |
| `MiniServices/Identity/IAuthService.cs` | Authentication service contract |
| `MiniServices/Identity/AuthService.cs` | Login/register implementation |
| `MiniServices/Billing/IBillingService.cs` | Billing service contract |
| `MiniServices/Billing/BillingService.cs` | Plan management implementation |
| `MiniServices/Products/IProductService.cs` | Product service contract |
| `MiniServices/Products/ProductService.cs` | Product CRUD implementation |
| `Extensions/ServiceCollectionExtensions.cs` | DI registration for Application layer |

---

### `src/Acme.SaaS.Infrastructure/`

Implements the contracts defined by the Application layer. Contains data access (EF Core), multi-tenancy, external services, and security.

#### File-by-File

| File | Description |
|---|---|
| `MultiTenancy/TenancyOptions.cs` | Tenancy mode: SeparateSchema or SharedSchema |
| `MultiTenancy/Services/TenantProvider.cs` | Reads current tenant from HttpContext.Items |
| `MultiTenancy/Services/SchemaService.cs` | Creates per-tenant SQL schemas |
| `MultiTenancy/Services/TenantConnectionService.cs` | Per-tenant connection strings (DB-per-tenant) |
| `MultiTenancy/Store/TenantStore.cs` | In-memory cache for tenant metadata |
| `Persistence/Contexts/MasterDbContext.cs` | Global DB: Tenants table only |
| `Persistence/Contexts/ApplicationDbContext.cs` | Business DB: schema-aware, 3 interceptors |
| `Persistence/Repositories/GenericRepository.cs` | Mode-aware CRUD with tenant filtering |
| `Persistence/Repositories/UnitOfWork.cs` | Transaction scope management |
| `Persistence/Configurations/TenantConfiguration.cs` | EF config for Tenant entity |
| `Persistence/Configurations/UserConfiguration.cs` | EF config for User entity |
| `Persistence/Configurations/ProductConfiguration.cs` | EF config for Product entity |
| `Persistence/Interceptors/AuditableEntityInterceptor.cs` | Auto-sets CreatedAt/UpdatedAt |
| `Persistence/Interceptors/SoftDeleteInterceptor.cs` | Converts DELETE to IsDeleted=true |
| `Persistence/Interceptors/TenantInterceptor.cs` | Auto-sets TenantId, prevents cross-tenant |
| `Persistence/Migrations/Master/` | Global schema migrations (run once) |
| `Persistence/Migrations/Tenant/` | Per-tenant schema migrations |
| `Services/Identity/JwtTokenService.cs` | JWT generation with tenant claims |
| `Services/Identity/CurrentUserService.cs` | Reads user info from JWT claims |
| `Services/Identity/PasswordHasher.cs` | BCrypt password hashing |
| `Services/FeatureGating/FeatureGatingService.cs` | Plan-based feature access control |
| `Services/CustomLogic/Strategies/ITaxCalculationStrategy.cs` | Per-tenant strategy interface |
| `Services/CustomLogic/Strategies/StandardTaxStrategy.cs` | Default tax: 10% |
| `Services/CustomLogic/Strategies/VodafoneTaxStrategy.cs` | Custom tax: 14% + 5 |
| `Services/CustomLogic/Factories/TaxStrategyFactory.cs` | Selects strategy per tenant |
| `Extensions/ServiceCollectionExtensions.cs` | DI registration for Infrastructure |

---

### `src/Acme.SaaS.API/`

The HTTP entry point. Contains controllers, middlewares, filters, and configuration.

#### File-by-File

| File | Description |
|---|---|
| `Controllers/BaseApiController.cs` | Base controller: ApiController, Route, ToActionResult helper |
| `Controllers/TenantsController.cs` | POST/GET tenants, GET list, PATCH deactivate |
| `Controllers/AuthController.cs` | POST login, POST register |
| `Controllers/ProductsController.cs` | POST/GET products, GET list |
| `Controllers/BillingController.cs` | GET plan, POST upgrade |
| `Middlewares/ExceptionHandlingMiddleware.cs` | Global exception → JSON response |
| `Middlewares/RequestLoggingMiddleware.cs` | Logs Method, Path, Status, Duration |
| `Middlewares/TenantResolutionMiddleware.cs` | Resolves tenant from Subdomain/Header/JWT |
| `Extensions/ServiceCollectionExtensions.cs` | DI: JWT, Swagger, Controllers, DbContexts |
| `Extensions/ApplicationBuilderExtensions.cs` | Middleware pipeline order |
| `Filters/SwaggerDefaultValues.cs` | Adds X-Tenant-Id to Swagger UI |
| `Program.cs` | Entry point (12 lines) |
| `appsettings.json` | Config: ConnectionStrings, Jwt, TenancyOptions, Logging |

---

## Test Structure (`tests/`)

### `tests/Acme.SaaS.Domain.Tests/`
Unit tests for domain entities, value objects, enums, and exceptions. No mocking needed.

### `tests/Acme.SaaS.Application.Tests/`
Tests for Mini-Services with mocked infrastructure interfaces (Moq/NSubstitute).

### `tests/Acme.SaaS.Infrastructure.Tests/`
Integration tests for EF Core, repositories, interceptors, and services. Uses InMemory database.

### `tests/Acme.SaaS.API.Tests/`
Integration tests for controllers and middlewares using `WebApplicationFactory` or `TestServer`.

---

## Key Architecture Points

### Dependency Direction
```
API → Application → Domain
  ↘  Infrastructure ↗  (implements Application interfaces)
```

### Multi-Tenancy Modes
| Feature | Separate Schema (default) | Shared Schema |
|---|---|---|
| Isolation | Schema-per-tenant | WHERE TenantId filter |
| GenericRepository | No filter needed | Auto-appends TenantId |
| ITenantEntity | Optional | Required |
| Migrations | Run per schema | Run once |

### Request Pipeline Order
1. ExceptionHandlingMiddleware
2. RequestLoggingMiddleware
3. TenantResolutionMiddleware ← resolves current tenant
4. Authentication (JWT)
5. Authorization
6. Controller → Mini-Service → DbContext
