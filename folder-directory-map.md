# Folder Directory Map

This document describes every directory and file in the Clean Architecture + CQRS structure.

## Solution Structure

```
Acme.SaaS.slnx
├── src/
│   ├── Acme.SaaS.Domain/           # Core business entities and rules
│   ├── Acme.SaaS.Application/      # CQRS: Commands, Queries, Handlers, Validators
│   ├── Acme.SaaS.Infrastructure/   # Data access, multi-tenancy, services
│   └── Acme.SaaS.API/              # HTTP entry point
├── tests/
│   ├── Acme.SaaS.Domain.Tests/
│   ├── Acme.SaaS.Application.Tests/
│   ├── Acme.SaaS.Infrastructure.Tests/
│   └── Acme.SaaS.API.Tests/
├── Directory.Build.props
├── Acme.SaaS.slnx
├── Dockerfile
├── docker-compose.yml
├── .gitignore
├── README.md
├── folder-directory-map.md
├── folder_structure.html
└── migration-changelog.md
```

---

## Source Structure (`src/`)

### `Acme.SaaS.Domain/`

The core business layer with **zero external dependencies**. Contains entities, value objects, enums, events, exceptions, and interfaces.

#### File-by-File

| File | Description |
|---|---|
| `Common/BaseEntity.cs` | Base class: `Id (Guid)`, `CreatedAt`, `CreatedBy` |
| `Common/BaseAuditableEntity.cs` | + `UpdatedAt`, `UpdatedBy`, `IsDeleted` |
| `Common/ValueObject.cs` | DDD Value Object base with structural equality |
| `Common/ITenantEntity.cs` | Interface for tenant-scoped entities |
| `Entities/Tenant.cs` | SaaS tenant: Identifier, SchemaName, Plan, Status |
| `Entities/User.cs` | Application user: Email, PasswordHash, Role, TenantId |
| `Entities/Role.cs` | Role definition: Name, Permissions |
| `Entities/Product.cs` | Sample business entity (tenant-scoped) |
| `Entities/Order.cs` | Order: UserId, TotalAmount, Status, OrderDate |
| `ValueObjects/Money.cs` | Amount + Currency with Add/Subtract |
| `ValueObjects/Address.cs` | Street, City, Country, PostalCode |
| `Enums/TenantStatus.cs` | Trial, Active, Suspended, Expired |
| `Enums/SubscriptionPlan.cs` | Free, Pro, Enterprise |
| `Enums/UserRole.cs` | SuperAdmin, TenantAdmin, Member |
| `Enums/OrderStatus.cs` | Pending, Processing, Shipped, Delivered, Cancelled |
| `Events/OrderCreatedEvent.cs` | Domain event when order is created |
| `Events/UserRegisteredEvent.cs` | Domain event when user registers |
| `Exceptions/DomainException.cs` | Base domain exception → 400 |
| `Exceptions/NotFoundException.cs` | Entity not found → 404 |
| `Interfaces/IRepository.cs` | Generic CRUD: GetById, GetAll, Add, Update, Delete |
| `Interfaces/IUnitOfWork.cs` | Transaction management |

---

### `Acme.SaaS.Application/`

The use-case layer. Uses **CQRS pattern** with MediatR. Contains Commands, Queries, Handlers, Validators, DTOs.

#### File-by-File

| File | Description |
|---|---|
| `Common/Interfaces/IApplicationDbContext.cs` | EF Core DbContext abstraction |
| `Common/Interfaces/ITenantProvider.cs` | Current tenant: Id, Schema, Plan |
| `Common/Interfaces/ICurrentUserService.cs` | Current user: Id, Email, Role |
| `Common/Interfaces/IPasswordHasher.cs` | BCrypt password hashing |
| `Common/Interfaces/IEmailService.cs` | Email sending: Send, Welcome, PasswordReset |
| `Common/Interfaces/IDateTime.cs` | DateTime abstraction for testing |
| `Common/Models/Result.cs` | Result pattern: Success/Failure with errors |
| `Common/Models/PaginatedList.cs` | Pagination: Items, TotalCount, Page, Size |
| `Common/Mappings/MappingProfile.cs` | AutoMapper: Entity ↔ DTO |
| `Common/Behaviours/LoggingBehaviour.cs` | MediatR pipeline: logs every request |
| `Common/Behaviours/ValidationBehaviour.cs` | MediatR pipeline: runs FluentValidation |
| `Common/Exceptions/ValidationException.cs` | Validation errors → 400 |
| `Common/Exceptions/ForbiddenException.cs` | Permission denied → 403 |
| `Features/Products/` | CQRS for Products feature |
| `Features/Products/Commands/CreateProduct/` | CreateProductCommand + Handler + Validator |
| `Features/Products/Commands/UpdateProduct/` | UpdateProductCommand + Handler + Validator |
| `Features/Products/Queries/GetProductById/` | GetProductByIdQuery + Handler |
| `Features/Products/Queries/GetProducts/` | GetProductsQuery + Handler |
| `Features/Products/DTOs/` | ProductDto, CreateProductDto |
| `Features/Users/` | RegisterUserCommand, LoginUserCommand, GetUserByIdQuery |
| `Features/Tenants/` | CreateTenant, DeactivateTenant, GetTenantById, GetTenants |
| `Features/Billing/` | GetCurrentPlan, UpgradePlan |
| `DependencyInjection.cs` | DI: MediatR, AutoMapper, FluentValidation, Behaviours |

---

### `Acme.SaaS.Infrastructure/`

Implements contracts. Contains data access, multi-tenancy, services, and identity.

#### File-by-File

| File | Description |
|---|---|
| `MultiTenancy/TenancyOptions.cs` | SeparateSchema / SharedSchema mode |
| `MultiTenancy/Services/TenantProvider.cs` | Reads tenant from HttpContext |
| `MultiTenancy/Services/SchemaService.cs` | Creates per-tenant schemas |
| `MultiTenancy/Services/TenantConnectionService.cs` | DB-per-tenant connection strings |
| `MultiTenancy/Store/TenantStore.cs` | In-memory tenant cache |
| `Persistence/Contexts/ApplicationDbContext.cs` | Business data context with interceptors |
| `Persistence/Contexts/MasterDbContext.cs` | Global tenant metadata context |
| `Persistence/Configurations/` | EF Core entity configurations |
| `Persistence/Repositories/GenericRepository.cs` | Mode-aware CRUD with tenant filtering |
| `Persistence/Repositories/UnitOfWork.cs` | Transaction management |
| `Persistence/Interceptors/` | AuditableEntity, SoftDelete, Tenant |
| `Persistence/Migrations/Master/` | Global schema migrations |
| `Persistence/Migrations/Tenant/` | Per-tenant schema migrations |
| `Services/EmailService.cs` | SMTP/SendGrid email sending |
| `Services/FileStorageService.cs` | S3/Azure Blob file storage |
| `Services/PaymentGatewayService.cs` | Stripe/PayPal payment processing |
| `Services/Identity/JwtTokenService.cs` | JWT generation |
| `Services/Identity/CurrentUserService.cs` | Reads user from JWT claims |
| `Services/Identity/PasswordHasher.cs` | BCrypt hashing |
| `Services/FeatureGating/` | Plan-based feature access |
| `Services/CustomLogic/` | Per-tenant strategy pattern |
| `Identity/ApplicationUser.cs` | Identity user model |
| `Identity/ApplicationRole.cs` | Identity role model |
| `Extensions/ServiceCollectionExtensions.cs` | DI registration |

---

### `Acme.SaaS.API/`

HTTP entry point. Thin Controllers, Middlewares, Filters, and configuration.

#### File-by-File

| File | Description |
|---|---|
| `Controllers/BaseApiController.cs` | Base controller with helpers |
| `Controllers/ProductsController.cs` | Product CRUD endpoints |
| `Controllers/UsersController.cs` | User management endpoints |
| `Controllers/AuthController.cs` | Login, Register, RefreshToken |
| `Controllers/TenantsController.cs` | Tenant management endpoints |
| `Controllers/BillingController.cs` | Subscription and billing endpoints |
| `Middlewares/ExceptionHandlingMiddleware.cs` | Global exception → JSON |
| `Middlewares/RequestLoggingMiddleware.cs` | Method, Path, Status, Duration |
| `Middlewares/TenantResolutionMiddleware.cs` | Subdomain/Header/JWT resolution |
| `Filters/SwaggerDefaultValues.cs` | Adds X-Tenant-Id to Swagger |
| `Models/ApiResponse.cs` | Standard response wrapper |
| `Models/PaginatedResponse.cs` | Pagination response model |
| `Resources/SharedResources.ar-EG.resx` | Arabic localization |
| `Resources/SharedResources.en-US.resx` | English localization |
| `Extensions/ServiceCollectionExtensions.cs` | API DI registration |
| `Extensions/ApplicationBuilderExtensions.cs` | Middleware pipeline |
| `Program.cs` | Entry point |
| `appsettings.json` | Configuration |
| `appsettings.Development.json` | Development configuration |

---

## Test Structure (`tests/`)

| Project | Type | Description |
|---|---|---|
| `Domain.Tests` | Pure Unit | Tests entities, value objects, events |
| `Application.Tests` | Unit + Mocks | Tests handlers, validators, behaviours |
| `Infrastructure.Tests` | Integration | Tests repositories, interceptors, services |
| `API.Tests` | E2E | Tests controllers, middlewares, endpoints |
