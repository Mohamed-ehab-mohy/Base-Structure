# Acme.SaaS.Infrastructure

The **Infrastructure** layer — implements Application interfaces. Handles data access, multi-tenancy, services, and identity.

## Key Features

- **Multi-Tenancy**: SeparateSchema (default), SharedSchema modes
- **EF Core**: ApplicationDbContext, MasterDbContext, 8+ entity configurations
- **Repositories**: `GenericRepository<T>` with tenant filtering, `UnitOfWork`
- **Interceptors**: AuditableEntity, SoftDelete, Tenant — EF Core save interceptors
- **Identity**: JWT generation, password hashing (BCrypt), CurrentUserService
- **Services**: Email (SMTP/SendGrid), FileStorage (S3/Azure), PaymentGateway (Stripe/PayPal)
- **Feature Gating**: Plan-based access control
- **Custom Logic**: Per-tenant strategy pattern
