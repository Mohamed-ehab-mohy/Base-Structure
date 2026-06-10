# Application Layer

**Location:** `src/Acme.SaaS.Application/`

The Application layer contains the business use cases and defines contracts (interfaces) that the Infrastructure layer implements. It depends only on the Domain layer — it has no knowledge of databases, APIs, or external services.

```
Acme.SaaS.Application/
├── Common/
│   ├── Behaviors/
│   │   ├── LoggingBehavior.cs       # Logs every operation (start, duration, status)
│   │   └── ValidationBehavior.cs    # FluentValidation pipeline
│   ├── DTOs/
│   │   ├── ApiResponse.cs           # Uniform API response wrapper
│   │   └── PagedResult.cs           # Pagination result model
│   ├── Exceptions/
│   │   ├── ValidationException.cs   # Input validation errors → 400
│   │   └── ForbiddenException.cs    # Permission denied → 403
│   ├── Interfaces/
│   │   ├── IApplicationDbContext.cs # EF Core DbContext abstraction
│   │   ├── ICurrentUserService.cs   # Current logged-in user info
│   │   ├── IGenericRepository.cs    # Generic CRUD operations
│   │   ├── IPasswordHasher.cs       # BCrypt password hashing
│   │   ├── ITenantProvider.cs       # Current tenant resolution
│   │   └── IUnitOfWork.cs           # Transaction management
│   └── Mapping/
│       └── MappingProfile.cs        # AutoMapper profiles
├── MiniServices/
│   ├── Billing/
│   │   ├── IBillingService.cs       # Billing service contract
│   │   ├── BillingService.cs        # Plan management implementation
│   │   └── ...                       # DTOs, requests, responses
│   ├── Identity/
│   │   ├── IAuthService.cs          # Authentication service contract
│   │   ├── AuthService.cs           # Login/register implementation
│   │   └── ...                       # DTOs, requests, responses
│   ├── Products/
│   │   ├── IProductService.cs       # Product service contract
│   │   ├── ProductService.cs        # Product CRUD implementation
│   │   └── ...                       # DTOs, requests, responses
│   └── Tenants/
│       ├── ITenantService.cs        # Tenant management contract
│       ├── TenantService.cs         # Tenant CRUD implementation
│       └── ...                       # DTOs, requests, responses
├── Extensions/
│   └── ServiceCollectionExtensions.cs  # DI registration for Application layer
├── Acme.SaaS.Application.csproj
└── README.md
```

## File-by-File Documentation

### Common/Interfaces/

#### `IApplicationDbContext.cs`
Abstracts the EF Core `DbContext` so the Application layer can query the database without a direct dependency on EF Core or the Infrastructure layer.

```csharp
public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<Product> Products { get; }
    DbSet<Role> Roles { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

---

#### `ITenantProvider.cs`
Provides information about the current tenant for the current HTTP request. Implemented by `TenantProvider` in Infrastructure.

| Method | Returns | Description |
|---|---|---|
| `GetTenantId()` | `Guid` | Current tenant's unique ID |
| `GetSchemaName()` | `string?` | Current tenant's schema name |
| `GetPlan()` | `SubscriptionPlan` | Current tenant's subscription plan |
| `GetIdentifier()` | `string?` | Current tenant's slug identifier |

---

#### `ICurrentUserService.cs`
Provides information about the currently authenticated user.

| Method | Returns | Description |
|---|---|---|
| `GetUserId()` | `Guid?` | Current user's ID |
| `GetUserEmail()` | `string?` | Current user's email |
| `GetRole()` | `string?` | Current user's role |
| `IsAuthenticated()` | `bool` | Whether the user is authenticated |

---

#### `IGenericRepository.cs`
Generic contract for data access operations. All database read/write goes through this interface.

```csharp
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}
```

---

#### `IUnitOfWork.cs`
Manages database transactions to ensure atomic operations.

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
```

---

#### `IPasswordHasher.cs`
Abstraction over password hashing to allow swapping algorithms.

```csharp
public interface IPasswordHasher
{
    string Hash(string password);       // Hash + salt a password
    bool Verify(string password, string hash); // Verify against stored hash
}
```

---

### Common/Behaviors/

#### `LoggingBehavior.cs`
Wraps every service operation with structured logging:
- Logs `"Starting operation: {Name}"` before execution
- Logs `"Completed operation: {Name} in {ElapsedMs}ms"` on success
- Logs `"Failed operation: {Name}"` with exception details on failure

Uses `ILogger<T>` for structured logging output.

---

#### `ValidationBehavior.cs`
Future-ready validation pipeline. When validators are registered for a request type, this behavior automatically runs them before the service executes:
- Collects all `IValidator<T>` implementations for the request type
- Runs all validators in parallel
- Throws `ValidationException` with all error messages if any validation fails

Currently available for integration when FluentValidation validators are added per Command.

---

### Common/DTOs/

#### `ApiResponse.cs`
The standard response wrapper for ALL API responses. Every endpoint returns this shape:

```json
{
  "success": true,
  "message": "Product created successfully.",
  "data": { ... },
  "errors": null
}
```

Static factory methods:
- `ApiResponse<T>.Ok(data, message)` — Successful response
- `ApiResponse<T>.Fail(message, errors)` — Error response

---

#### `PagedResult.cs`
Standard pagination wrapper for list endpoints:

```json
{
  "items": [ ... ],
  "totalCount": 100,
  "page": 1,
  "size": 10,
  "totalPages": 10
}
```

---

### Common/Exceptions/

#### `ValidationException.cs`
Thrown when input data fails validation. Contains a list of individual error messages. The `ExceptionHandlingMiddleware` converts this to **400 Bad Request** with the errors list.

#### `ForbiddenException.cs`
Thrown when an authenticated user tries to access a resource they don't have permission for. Converted to **403 Forbidden** by the middleware.

---

### Common/Mapping/

#### `MappingProfile.cs`
AutoMapper profile that defines how entities map to DTOs and vice versa:

```csharp
CreateMap<Tenant, TenantDto>().ReverseMap();
CreateMap<Product, ProductDto>().ReverseMap();
```

Add new mappings here when creating new entities/DTOs.

---

### MiniServices/

The Application layer uses **Mini-Services** instead of full CQRS/MediatR. Each feature is a self-contained module with:
- An **Interface** defining the service contract
- An **Implementation** class containing the business logic
- **DTOs/Records** for request/response models

#### Tenants/ (`ITenantService` + `TenantService`)
Manages the lifecycle of tenants:

| Method | Description |
|---|---|
| `CreateTenantAsync(request, ct)` | Creates a new tenant, generates schema name, sets default plan/status |
| `GetTenantByIdAsync(tenantId, ct)` | Gets tenant details by ID |
| `GetTenantsListAsync(page, size, ct)` | Paginated list of all tenants |
| `DeactivateTenantAsync(tenantId, ct)` | Suspends a tenant account |

#### Identity/ (`IAuthService` + `AuthService`)
Handles user authentication:

| Method | Description |
|---|---|
| `LoginAsync(request, ct)` | Authenticates user by email/password, returns JWT |
| `RegisterAsync(request, ct)` | Creates a new user account within the current tenant |

Uses `ITenantProvider` to scope users to the correct tenant and `IPasswordHasher` for secure password storage.

#### Billing/ (`IBillingService` + `BillingService`)
Manages subscription and billing:

| Method | Description |
|---|---|
| `GetCurrentPlanAsync(ct)` | Returns the current tenant's subscription plan |
| `UpgradePlanAsync(request, ct)` | Upgrades/downgrades the tenant's plan |

#### Products/ (`IProductService` + `ProductService`)
Sample CRUD service demonstrating tenant-scoped operations:

| Method | Description |
|---|---|
| `CreateProductAsync(request, ct)` | Creates a new product for the current tenant |
| `GetProductByIdAsync(id, ct)` | Gets a product by ID (scoped to tenant) |
| `GetProductsListAsync(page, size, ct)` | Paginated list of products (current tenant only) |

All queries automatically filter by `TenantId` via `ITenantProvider`.

---

### Extensions/

#### `ServiceCollectionExtensions.cs`
Registers all Application layer services in the DI container:

```csharp
services.AddAutoMapper(typeof(MappingProfile));
services.AddValidatorsFromAssemblyContaining<MappingProfile>();
services.AddScoped<ITenantService, TenantService>();
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IBillingService, BillingService>();
```

Called once from `Program.cs`:
```csharp
builder.Services.AddApplicationLayer();
```
