# Infrastructure Layer

**Location:** `src/Acme.SaaS.Infrastructure/`

The Infrastructure layer implements the contracts (interfaces) defined by the Application layer. It contains data access (EF Core), multi-tenancy logic, external service integrations, and security services.

```
Acme.SaaS.Infrastructure/
├── Extensions/
│   └── ServiceCollectionExtensions.cs  # DI registration for Infrastructure
├── MultiTenancy/
│   ├── TenancyOptions.cs               # SeparateSchema / SharedSchema mode
│   ├── Services/
│   │   ├── TenantProvider.cs            # Resolves current tenant from HttpContext
│   │   ├── SchemaService.cs             # Creates per-tenant database schemas
│   │   └── TenantConnectionService.cs   # Manages per-tenant connection strings
│   └── Store/
│       └── TenantStore.cs               # In-memory cache for tenant info
├── Persistence/
│   ├── Configurations/
│   │   ├── TenantConfiguration.cs        # EF config for Tenant entity
│   │   ├── UserConfiguration.cs          # EF config for User entity
│   │   └── ProductConfiguration.cs       # EF config for Product entity
│   ├── Contexts/
│   │   ├── ApplicationDbContext.cs       # Primary business data context
│   │   └── MasterDbContext.cs            # Global tenant metadata context
│   ├── Interceptors/
│   │   ├── AuditableEntityInterceptor.cs # Auto-sets CreatedAt/UpdatedAt
│   │   ├── SoftDeleteInterceptor.cs      # Converts DELETE to IsDeleted=true
│   │   └── TenantInterceptor.cs          # Auto-sets TenantId on new entities
│   ├── Migrations/
│   │   ├── Master/                       # Tenant metadata migrations (run once)
│   │   └── Tenant/                       # Business data migrations (per-schema)
│   └── Repositories/
│       ├── GenericRepository.cs          # Mode-aware CRUD (Separate/Shared Schema)
│       └── UnitOfWork.cs                 # Transaction management
├── Services/
│   ├── CustomLogic/
│   │   ├── Factories/
│   │   │   └── TaxStrategyFactory.cs     # Selects strategy per tenant
│   │   └── Strategies/
│   │       ├── ITaxCalculationStrategy.cs # Tax calculation contract
│   │       ├── StandardTaxStrategy.cs     # Default: 10% tax
│   │       └── VodafoneTaxStrategy.cs     # Custom: 14% + 5 flat
│   ├── FeatureGating/
│   │   └── FeatureGatingService.cs       # Plan-based feature access control
│   └── Identity/
│       ├── CurrentUserService.cs         # Extracts user info from JWT claims
│       ├── JwtTokenService.cs            # JWT generation with tenant claims
│       └── PasswordHasher.cs             # BCrypt password hashing
├── Acme.SaaS.Infrastructure.csproj
└── README.md
```

## File-by-File Documentation

### MultiTenancy/

The core of the multi-tenant architecture. All tenant isolation logic lives here.

#### `TenancyOptions.cs`
Configuration object read from `appsettings.json`:

```json
{
  "TenancyOptions": {
    "Mode": "SeparateSchema"
  }
}
```

**Modes:**
- `SeparateSchema` (default) — Each tenant gets its own schema. No `TenantId` filtering needed.
- `SharedSchema` — All tenants share the same tables. `GenericRepository` auto-appends `WHERE TenantId = @id`.

---

#### `Services/TenantProvider.cs`
Implements `ITenantProvider`. Reads tenant information from `HttpContext.Items` — which was populated by `TenantResolutionMiddleware`.

| Method | Source |
|---|---|
| `GetTenantId()` | `HttpContext.Items["TenantId"]` |
| `GetSchemaName()` | `HttpContext.Items["SchemaName"]` |
| `GetPlan()` | `HttpContext.Items["TenantPlan"]` |
| `GetIdentifier()` | `HttpContext.Items["TenantIdentifier"]` |

Registered as `Scoped` — one instance per HTTP request.

---

#### `Services/SchemaService.cs`
Creates database schemas for new tenants:
```sql
CREATE SCHEMA [tenant_{identifier}]
```
Returns the generated schema name for use in entity configuration.

---

#### `Services/TenantConnectionService.cs`
Manages per-tenant connection strings for **DB-per-tenant** scenarios. In the current **Schema-per-tenant** setup, all tenants share the same connection string (the Master DB connection), and isolation is handled by schema.

---

#### `Store/TenantStore.cs`
In-memory cache (`ConcurrentDictionary<string, Tenant>`) for tenant metadata:
- `Get(identifier)` — Retrieve tenant from cache
- `Set(tenant)` — Store tenant in cache
- `Remove(identifier)` — Evict tenant from cache
- `Clear()` — Clear entire cache

Prevents a database round-trip on every request. Can be swapped for Redis in production.

---

### Persistence/

#### `Contexts/MasterDbContext.cs`
DbContext for the **global master database**. Contains only tenant metadata:

```csharp
public DbSet<Tenant> Tenants => Set<Tenant>();
```

Configured with a unique index on `Tenant.Identifier`. Migrations for this context run **once** (not per-tenant).

---

#### `Contexts/ApplicationDbContext.cs`
DbContext for **business data**. Implements `IApplicationDbContext`. Key behaviors:
- Applies Entity Type Configurations from `Configurations/` folder
- In **Separate Schema** mode: calls `HasDefaultSchema(tenantSchemaName)` to isolate each tenant
- Uses three interceptors: `AuditableEntityInterceptor`, `SoftDeleteInterceptor`, `TenantInterceptor`

Injected with `ITenantProvider` to determine the correct schema per request.

---

#### `Repositories/GenericRepository.cs`
Generic CRUD repository with **mode-aware tenant filtering**:

| Operation | Separate Schema | Shared Schema |
|---|---|---|
| `GetByIdAsync` | No filter | Validates `TenantId` match |
| `GetAllAsync` | No filter | Auto-appends `WHERE TenantId = @id` |
| `FindAsync` | No filter | Auto-appends `WHERE TenantId = @id` |
| `AnyAsync` | No filter | Auto-appends `WHERE TenantId = @id` |

Uses expression trees to dynamically build the `WHERE TenantId` clause at runtime.

---

#### `Repositories/UnitOfWork.cs`
Implements `IUnitOfWork` for managing database transactions:

```csharp
await unitOfWork.BeginTransactionAsync(ct);
try {
    // ... multiple operations ...
    await unitOfWork.CommitTransactionAsync(ct);
} catch {
    await unitOfWork.RollbackTransactionAsync(ct);
}
```

---

#### `Migrations/Master/` and `Migrations/Tenant/`
Two separate migration directories:
- **Master/**: Global tables (Tenants, Plans). Run once via `dotnet ef database update`.
- **Tenant/**: Per-tenant business tables. Run for each new tenant schema.

---

#### `Configurations/`

##### `TenantConfiguration.cs`
Configures the `Tenants` table:
- Unique index on `Identifier`
- Property constraints: `Identifier` (100 chars), `SchemaName` (200 chars), `ConnectionString` (500 chars)
- Enum conversions for `Plan` and `Status` (stored as strings)

##### `UserConfiguration.cs`
Configures the `Users` table:
- Unique composite index on `(Email, TenantId)` — no duplicate emails within a tenant
- Property constraints: `Email` (256 chars), `Role` (50 chars)
- `PasswordHash` stored as required

##### `ProductConfiguration.cs`
Configures the `Products` table:
- Index on `TenantId` for efficient tenant scoping
- `Price` stored as `decimal(18,2)`
- `Name` required, max 200 characters

---

#### `Interceptors/`

##### `AuditableEntityInterceptor.cs`
EF Core `SaveChangesInterceptor` that automatically:
- Sets `CreatedAt` and `CreatedBy` when an entity is first added
- Sets `UpdatedAt` and `UpdatedBy` when an entity is modified

##### `SoftDeleteInterceptor.cs`
EF Core `SaveChangesInterceptor` that:
- Detects entities being deleted
- Changes the state from `Deleted` to `Modified`
- Sets `IsDeleted = true`

##### `TenantInterceptor.cs`
EF Core `SaveChangesInterceptor` responsible for tenant isolation:
- **On Added**: Automatically sets `TenantId` on `ITenantEntity` implementations
- **On Modified (Shared Schema only)**: Prevents changing `TenantId` (throws `DomainException`)

---

### Services/

#### `Identity/JwtTokenService.cs`
Generates JWT tokens for authenticated users. Token payload includes:
- `ClaimTypes.NameIdentifier` — User ID
- `ClaimTypes.Email` — User email
- `ClaimTypes.Role` — User role
- `"TenantId"` — Current tenant ID

Uses HMAC SHA-256 signing. Configuration options: `Secret`, `Issuer`, `Audience`, `ExpirationHours`.

---

#### `Identity/CurrentUserService.cs`
Implements `ICurrentUserService`. Extracts user information from the current HTTP context's JWT claims.

---

#### `Identity/PasswordHasher.cs`
Implements `IPasswordHasher` using BCrypt (via `BCrypt.Net-Next`):
- `Hash(password)` — Generates a salted BCrypt hash
- `Verify(password, hash)` — Verifies a password against its stored hash

---

#### `FeatureGating/FeatureGatingService.cs`
Controls feature access based on the tenant's subscription plan:

```csharp
FeaturePlanMap = {
    ["basic-crud"]     => Free,
    ["export"]         => Pro,
    ["api-access"]     => Pro,
    ["audit-logs"]     => Enterprise,
    ["custom-branding"] => Enterprise
}
```

Usage: `featureGatingService.IsFeatureAllowed("export")` → returns `true` only for Pro+ plans.

---

#### `CustomLogic/Strategies/`

##### `ITaxCalculationStrategy`
Strategy interface for per-tenant business logic:

```csharp
public interface ITaxCalculationStrategy
{
    decimal CalculateTax(decimal amount);
}
```

##### `StandardTaxStrategy`
Default tax calculation: **10%** of the amount (`amount * 0.10m`).

##### `VodafoneTaxStrategy`
Custom tax calculation for Vodafone: **14% + 5 EGP** (`(amount * 0.14m) + 5`).

---

#### `CustomLogic/Factories/TaxStrategyFactory.cs`
Factory that selects the correct tax strategy per tenant:

```csharp
public ITaxCalculationStrategy GetStrategy()
{
    return tenantIdentifier switch
    {
        "vodafone" => _vodafoneStrategy,
        _          => _standardStrategy
    };
}
```

New tenants get their own strategy without modifying existing code.

---

### Extensions/

#### `ServiceCollectionExtensions.cs`
Registers all Infrastructure services in the DI container:

```csharp
services.AddDbContext<MasterDbContext>(...);
services.AddDbContext<ApplicationDbContext>(...);  // With interceptors
services.AddScoped<ITenantProvider, TenantProvider>();
services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<IFeatureGatingService, FeatureGatingService>();
services.AddScoped<JwtTokenService>();
services.AddScoped<ICurrentUserService, CurrentUserService>();
services.AddScoped<IPasswordHasher, PasswordHasher>();
services.AddSingleton<TenantStore>();
// ... and interceptors, strategies, factories
```

Called from the API layer's `AddTenantDbContext` method.
