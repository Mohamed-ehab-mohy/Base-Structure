# Acme.SaaS — .NET Clean Architecture with CQRS

A production-ready **Clean Architecture** template built with **.NET 10** following **CQRS pattern** with **MediatR**, **FluentValidation**, and **AutoMapper**. Designed for scalable SaaS applications with multi-tenancy support.

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           Acme.SaaS.slnx                                        │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│  📦 Acme.SaaS.Domain           // Zero external dependencies                    │
│  ├── Common/                   // BaseEntity, BaseAuditableEntity, ValueObject  │
│  ├── Entities/                 // Tenant, User, Product, Order, Role           │
│  ├── ValueObjects/             // Money, Address                                │
│  ├── Enums/                    // TenantStatus, SubscriptionPlan, UserRole,     │
│  │                                OrderStatus                                   │
│  ├── Events/                   // OrderCreatedEvent, UserRegisteredEvent        │
│  ├── Exceptions/               // DomainException, NotFoundException            │
│  └── Interfaces/               // IRepository<T>, IUnitOfWork                   │
│                                                                                 │
│  📦 Acme.SaaS.Application      // CQRS + MediatR                                │
│  ├── Common/                                                                   │
│  │   ├── Interfaces/          // IApplicationDbContext, ITenantProvider,        │
│  │   │                           ICurrentUserService, IPasswordHasher,          │
│  │   │                           IEmailService, IDateTime                       │
│  │   ├── Models/              // Result<T>, PaginatedList<T>                    │
│  │   ├── Mappings/            // MappingProfile (AutoMapper)                    │
│  │   ├── Behaviours/          // LoggingBehaviour, ValidationBehaviour          │
│  │   └── Exceptions/          // ValidationException, ForbiddenException        │
│  ├── Features/                // ⭐ CQRS Modules                                │
│  │   ├── Products/            // Commands/, Queries/, DTOs/                     │
│  │   ├── Users/               // RegisterUserCommand, LoginUserCommand, ...     │
│  │   ├── Tenants/             // CreateTenantCommand, GetTenantsQuery, ...      │
│  │   └── Billing/             // GetCurrentPlanQuery, UpgradePlanCommand        │
│  └── DependencyInjection.cs   // MediatR + AutoMapper + FluentValidation        │
│                                                                                 │
│  📦 Acme.SaaS.Infrastructure  // Implementations                                │
│  ├── MultiTenancy/            // TenancyOptions, TenantProvider, SchemaService  │
│  ├── Persistence/             // Contexts, Repositories, Configurations,        │
│  │                               Interceptors, Migrations                       │
│  ├── Services/                // EmailService, FileStorageService,              │
│  │                               PaymentGatewayService                          │
│  ├── Identity/                // ApplicationUser, ApplicationRole               │
│  └── Extensions/              // DI registration                                │
│                                                                                 │
│  📦 Acme.SaaS.API             // Presentation Layer                             │
│  ├── Controllers/             // ProductsController, AuthController, ...        │
│  ├── Middlewares/             // ExceptionHandling, RequestLogging,             │
│  │                               TenantResolution                               │
│  ├── Filters/                 // SwaggerDefaultValues                           │
│  ├── Models/                  // ApiResponse, PaginatedResponse                 │
│  ├── Resources/               // Localization (.resx)                           │
│  ├── Extensions/              // ServiceCollectionExtensions,                   │
│  │                               ApplicationBuilderExtensions                    │
│  └── Program.cs               // Entry Point                                    │
│                                                                                 │
│  📦 Tests (xUnit)                                                                │
│  ├── Domain.Tests                                                                │
│  ├── Application.Tests                                                          │
│  ├── Infrastructure.Tests                                                       │
│  └── API.Tests                                                                  │
│                                                                                 │
├─────────────────────────────────────────────────────────────────────────────────┤
│  Runtime: .NET 10  |  DB: SQL Server  |  Auth: JWT Bearer                       │
│  CQRS: MediatR  |  Validation: FluentValidation  |  Mapping: AutoMapper          │
└─────────────────────────────────────────────────────────────────────────────────┘
```

## Architecture

### Clean Architecture Layers

1. **Domain Layer** — Entities, Value Objects, Enums, Events, Exceptions, Interfaces. Zero dependencies.
2. **Application Layer** — CQRS with MediatR (Commands, Queries, Handlers, Validators). Depends on Domain only.
3. **Infrastructure Layer** — EF Core, Repositories, External Services, Identity. Implements Application interfaces.
4. **Presentation Layer** — ASP.NET Core Web API. Thin Controllers → MediatR.

### CQRS Pattern

- **Commands**: تغيير Data (Create, Update, Delete)
- **Queries**: قراءة Data بس (Get, List)
- **Handlers**: Logic لكل Command/Query
- **Validators**: FluentValidation لكل Command
- **Pipeline Behaviours**: Logging + Validation قبل الـ Handler

### Multi-Tenancy

Configurable via `appsettings.json`:
- `SeparateSchema` (default) — Schema-per-tenant
- `SharedSchema` — Filter-per-tenant (`WHERE TenantId`)

### Flow

```
HTTP Request → ExceptionMiddleware → LoggingMiddleware → TenantMiddleware
→ JWT Auth → Controller → MediatR → Handler → Repository → DB
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (LocalDB, Docker, or remote)

### Setup

```bash
git clone https://github.com/Mohamed-ehab-mohy/Base-Structure.git
cd Base-Structure
dotnet restore
dotnet build
dotnet run --project src/Acme.SaaS.API
```

### With Docker

```bash
docker-compose up
```

## Documentation

- `folder_structure.html` — Interactive visual map
- `folder-directory-map.md` — Directory and file responsibilities
- `migration-changelog.md` — Architecture decisions and changelog
- Layer-specific README.md files in each project
