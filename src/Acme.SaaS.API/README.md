# API Layer (Presentation)

**Location:** `src/Acme.SaaS.API/`

The Presentation layer is the HTTP entry point of the application. It contains Controllers, Middlewares, Filters, and configuration. This layer has **no business logic** — it delegates all work to the Application layer's Mini-Services.

```
Acme.SaaS.API/
├── Controllers/
│   ├── BaseApiController.cs           # Base controller with shared helpers
│   ├── AuthController.cs              # Authentication endpoints
│   ├── BillingController.cs           # Subscription management
│   ├── ProductsController.cs          # Product CRUD (sample)
│   └── TenantsController.cs           # Tenant management
├── Middlewares/
│   ├── ExceptionHandlingMiddleware.cs  # Global error handling
│   ├── RequestLoggingMiddleware.cs     # Request/response logging
│   └── TenantResolutionMiddleware.cs   # ⭐ Tenant resolution from HTTP
├── Extensions/
│   ├── ApplicationBuilderExtensions.cs # Pipeline configuration
│   └── ServiceCollectionExtensions.cs  # DI and service registration
├── Filters/
│   └── SwaggerDefaultValues.cs         # Swagger default parameters
├── Program.cs                          # Application entry point
├── appsettings.json                    # Production configuration
├── appsettings.Development.json        # Development configuration
├── Acme.SaaS.API.csproj
└── README.md
```

## File-by-File Documentation

### Controllers/

#### `BaseApiController.cs`
Abstract base controller that all API controllers inherit from:
- `[ApiController]` attribute — automatic model validation, binding
- `[Route("api/[controller]")]` — conventional routing
- `ToActionResult(ApiResponse<T>)` — converts service responses to HTTP responses:
  - `Success = true` → `200 OK`
  - `Success = false` → `400 Bad Request`

---

#### `TenantsController.cs`
Manage tenant lifecycle. All endpoints require SuperAdmin authorization.

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/tenants` | Create a new tenant |
| `GET` | `/api/tenants/{id}` | Get tenant by ID |
| `GET` | `/api/tenants?page=1&size=10` | Paginated list of tenants |
| `PATCH` | `/api/tenants/{id}/deactivate` | Deactivate (suspend) a tenant |

---

#### `AuthController.cs`
User authentication within a tenant context.

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/auth/login` | Login with email + password → JWT token |
| `POST` | `/api/auth/register` | Register a new user within the current tenant |

---

#### `ProductsController.cs`
Sample tenant-scoped CRUD controller.

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/products` | Create a new product |
| `GET` | `/api/products/{id}` | Get product by ID |
| `GET` | `/api/products?page=1&size=10` | Paginated list of products (tenant-scoped) |

---

#### `BillingController.cs`
Subscription management for the current tenant.

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/billing/plan` | Get current subscription plan |
| `POST` | `/api/billing/upgrade` | Upgrade/downgrade subscription plan |

---

### Middlewares/

#### `ExceptionHandlingMiddleware.cs`
**Pipeline position**: First middleware (outermost).
**Purpose**: Global exception handler — catches all unhandled exceptions and returns structured JSON responses.

| Exception | HTTP Status | Response |
|---|---|---|
| `NotFoundException` | 404 Not Found | `{ Success: false, Message: "..." }` |
| `DomainException` | 400 Bad Request | `{ Success: false, Message: "..." }` |
| `ValidationException` | 400 Bad Request | `{ Success: false, Message: "...", Errors: [...] }` |
| `ForbiddenException` | 403 Forbidden | `{ Success: false, Message: "..." }` |
| Any other `Exception` | 500 Internal Server Error | `{ Success: false, Message: "An internal error occurred." }` |

---

#### `RequestLoggingMiddleware.cs`
**Pipeline position**: Second middleware.
**Purpose**: Logs every HTTP request with timing information.

```
{Method} {Path} responded {StatusCode} in {ElapsedMs}ms
```
Example: `GET /api/products responded 200 in 45ms`

Uses `Stopwatch` for high-precision timing.

---

#### `TenantResolutionMiddleware.cs`
**Pipeline position**: Third middleware (after logging, before authentication).
**Purpose**: Resolves the current tenant from the incoming request and stores it in `HttpContext.Items`.

**Tenant Resolution Strategy (in order):**
1. **Subdomain**: `tenant1.acme.com` → extracts `tenant1`
2. **Header**: `X-Tenant-Id` HTTP header
3. **JWT Claim**: `TenantId` claim from JWT token

**What gets stored in `HttpContext.Items`:**
- `"TenantId"` → `Guid`
- `"TenantIdentifier"` → `string` (e.g., "acme-corp")
- `"SchemaName"` → `string` (e.g., "tenant_acmecorp")
- `"TenantPlan"` → `SubscriptionPlan`
- `"TenantStatus"` → `TenantStatus`

**Caching**: After resolving the tenant from the database, it caches the result in `TenantStore` (in-memory) to avoid repeated database calls.

---

### Extensions/

#### `ServiceCollectionExtensions.cs`
Registers all services for the API layer:

```csharp
// AddApiLayer: JWT auth, Swagger, Controllers, HttpContextAccessor
builder.Services.AddApiLayer(configuration);

// AddApplicationLayer: Mini-Services, AutoMapper, FluentValidation
builder.Services.AddApplicationLayer();

// AddTenantDbContext: DbContexts, Repositories, Tenant services
builder.Services.AddTenantDbContext(configuration);
```

**`AddApiLayer`** configures:
- `IHttpContextAccessor` — required by `CurrentUserService` and `TenantProvider`
- JWT Authentication with Bearer token validation
- Authorization policies
- Swagger/OpenAPI with JWT security definition
- MVC Controllers

**`AddTenantDbContext`** configures:
- Reads connection string from `appsettings.json`
- Reads `TenancyOptions` from configuration
- Registers `TenancyOptions` as singleton
- Calls `AddInfrastructureLayer(connectionString)` from the Infrastructure project

---

#### `ApplicationBuilderExtensions.cs`
Configures the middleware pipeline:

```csharp
app.UseExceptionHandlingMiddleware();   // 1. Global error handling
app.UseRequestLoggingMiddleware();      // 2. Request logging
app.UseTenantResolutionMiddleware();    // 3. ⭐ Tenant resolution
app.UseAuthentication();                // 4. JWT authentication
app.UseAuthorization();                 // 5. Authorization
app.UseSwagger();                       // 6. Swagger UI (development)
app.MapControllers();                   // 7. Route to controllers
```

---

### Filters/

#### `SwaggerDefaultValues.cs`
`IOperationFilter` that automatically adds the `X-Tenant-Id` header parameter to all Swagger operations. This makes it easy to test multi-tenant endpoints directly from the Swagger UI.

---

### Configuration

#### `Program.cs`
Minimal entry point (12 lines):

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApiLayer(builder.Configuration);
builder.Services.AddApplicationLayer();
builder.Services.AddTenantDbContext(builder.Configuration);
var app = builder.Build();
app.UseApiPipeline();
app.Run();
```

#### `appsettings.json`
Key configuration sections:
- `ConnectionStrings.MasterDb` — SQL Server connection string
- `Jwt` — Secret, Issuer, Audience, ExpirationHours
- `TenancyOptions.Mode` — SeparateSchema or SharedSchema
- `Logging` — Log levels
