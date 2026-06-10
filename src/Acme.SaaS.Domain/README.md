# Acme.SaaS.Domain

The **Domain** layer — zero external dependencies. Contains the core business entities, value objects, enums, domain events, exceptions, and repository contracts.

## Key Concepts

- **Entities**: Tenant, User, Product, Order, Role — have identity (`Id`)
- **Value Objects**: Money, Address — immutable, equality by value
- **Enums**: TenantStatus, SubscriptionPlan, UserRole, OrderStatus
- **Domain Events**: OrderCreatedEvent, UserRegisteredEvent — side effects
- **Exceptions**: DomainException → 400, NotFoundException → 404
- **Interfaces**: `IRepository<T>`, `IUnitOfWork` — contracts defined here, implemented in Infrastructure
