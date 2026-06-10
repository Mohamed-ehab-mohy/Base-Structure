# Domain Layer

**Location:** `src/Acme.SaaS.Domain/`

The Domain layer is the innermost layer of the Clean Architecture — it has **zero external dependencies**. It contains the enterprise business rules, entities, enums, exceptions, and shared abstractions.

```
Acme.SaaS.Domain/
├── Common/
│   ├── BaseEntity.cs              # Base class for all entities (Id, CreatedAt, CreatedBy)
│   ├── BaseAuditableEntity.cs     # Extends BaseEntity (UpdatedAt, UpdatedBy, IsDeleted)
│   ├── ValueObject.cs             # DDD Value Object base (equality by components)
│   └── ITenantEntity.cs           # Interface for entities scoped to a tenant
├── Entities/
│   ├── Tenant.cs                  # SaaS tenant (Identifier, Schema, Plan, Status)
│   ├── User.cs                    # Application user (Email, PasswordHash, Role, TenantId)
│   ├── Role.cs                    # Role definition (Name, Permissions)
│   └── Product.cs                 # Sample business entity
├── Enums/
│   ├── TenantStatus.cs            # Trial, Active, Suspended, Expired
│   ├── SubscriptionPlan.cs        # Free, Pro, Enterprise
│   └── UserRole.cs                # SuperAdmin, TenantAdmin, Member
├── Exceptions/
│   ├── DomainException.cs         # Base domain exception
│   └── NotFoundException.cs       # Entity not found (404)
├── Acme.SaaS.Domain.csproj        # No package dependencies
└── README.md
```

## File-by-File Documentation

### Common/

#### `BaseEntity.cs`
The foundation for every entity in the system. Provides:
- **`Id (Guid)`**: Auto-generated unique identifier. Uses `Guid.NewGuid()` so IDs can be generated client-side before persistence.
- **`CreatedAt (DateTime)`**: UTC timestamp set automatically when the entity is first persisted via `AuditableEntityInterceptor`.
- **`CreatedBy (string?)`**: Optional reference to who created the entity.

All domain entities should inherit from `BaseEntity` (or `BaseAuditableEntity` for entities that need audit trail).

---

#### `BaseAuditableEntity.cs`
Extends `BaseEntity` with full audit capabilities:
- **`UpdatedAt (DateTime?)`**: UTC timestamp set automatically when the entity is modified.
- **`UpdatedBy (string?)`**: Reference to who last modified the entity.
- **`IsDeleted (bool)`**: Soft-delete flag. When a user deletes an entity, the `SoftDeleteInterceptor` sets this to `true` instead of physically removing the row.

---

#### `ValueObject.cs`
Base class for DDD Value Objects (immutable objects identified by their properties rather than an ID):
- **`GetEqualityComponents()`**: Subclasses override this to define which properties determine equality.
- **`Equals()` / `GetHashCode()`**: Uses structural equality — two Value Objects with the same components are considered equal.
- **`==` / `!=` operators**: Structural equality operators.

Example: `Money(Amount: 100, Currency: "USD")` — two Money objects with the same amount and currency are equal.

---

#### `ITenantEntity.cs`
Interface for entities that belong to a specific tenant. Used in **Shared Schema** mode:
- **`TenantId (Guid)`**: Associates the entity with a tenant.
- The `GenericRepository` auto-filters queries by `TenantId` when the entity implements this interface.
- In **Separate Schema** mode, this is optional (isolation is handled by schema).

---

### Entities/

#### `Tenant.cs`
The core multi-tenant entity. Inherits `BaseAuditableEntity`.

| Property | Type | Description |
|---|---|---|
| `Identifier` | `string` | Unique tenant slug (e.g., "acme-corp"). Used in subdomain resolution. |
| `SchemaName` | `string?` | Database schema name for this tenant (e.g., "tenant_acmecorp"). |
| `ConnectionString` | `string?` | Optional per-tenant connection string (for DB-per-tenant). |
| `Plan` | `SubscriptionPlan` | Current subscription plan: Free, Pro, or Enterprise. |
| `Status` | `TenantStatus` | Account status: Trial, Active, Suspended, or Expired. |

---

#### `User.cs`
Application user entity. Inherits `BaseAuditableEntity` and implements `ITenantEntity`.

| Property | Type | Description |
|---|---|---|
| `Email` | `string` | User email address (unique within a tenant). |
| `PasswordHash` | `string` | BCrypt hash of the password. Never store plain text. |
| `IsActive` | `bool` | Whether the user account is active. |
| `Role` | `string` | User role: "SuperAdmin", "TenantAdmin", or "Member". |
| `TenantId` | `Guid` | The tenant this user belongs to. |

---

#### `Role.cs`
Role definition entity. Separated from User to support flexible permissions.

| Property | Type | Description |
|---|---|---|
| `Id` | `Guid` | Unique identifier. |
| `Name` | `string` | Role name (SuperAdmin, TenantAdmin, Member). |
| `Permissions` | `List<string>` | List of permission strings for fine-grained access control. |

---

#### `Product.cs`
Sample business entity demonstrating a tenant-scoped entity. Inherits `BaseAuditableEntity` and implements `ITenantEntity`.

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Product name. |
| `Description` | `string?` | Optional product description. |
| `Price` | `decimal` | Product price. |
| `TenantId` | `Guid` | The tenant this product belongs to. |

---

### Enums/

#### `TenantStatus.cs`
```csharp
Trial = 0      // New tenant in trial period
Active = 1     // Active subscription
Suspended = 2  // Account suspended (admin action or payment failure)
Expired = 3    // Trial/Subscription period expired
```

#### `SubscriptionPlan.cs`
```csharp
Free = 0        // Basic CRUD only
Pro = 1         // Export, API access
Enterprise = 2  // Audit logs, custom branding, priority support
```

#### `UserRole.cs`
```csharp
SuperAdmin = 0   // System-wide admin (across all tenants)
TenantAdmin = 1  // Admin within a specific tenant
Member = 2       // Regular user within a tenant
```

---

### Exceptions/

#### `DomainException.cs`
Base exception for all domain-level errors. All custom domain exceptions should inherit from this. The `ExceptionHandlingMiddleware` catches these and returns **400 Bad Request**.

#### `NotFoundException.cs`
Thrown when an entity is not found. Inherits `DomainException`. The middleware catches this and returns **404 Not Found**.

```csharp
throw new NotFoundException("Product", productId);
// Response: { Success: false, Message: "'Product' with Id '...' was not found." }
```

---

## Design Rules

1. **Zero dependencies**: The Domain layer must never reference external libraries, frameworks, or other projects.
2. **Business logic first**: Encapsulate business rules in entity methods, not in services.
3. **Persistence ignorance**: Entities should not have EF Core attributes or database concerns.
4. **Tenant-agnostic**: The Domain layer should not reference tenant concepts directly — `ITenantEntity` is a lightweight marker interface.
