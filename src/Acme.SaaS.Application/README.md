# Acme.SaaS.Application

The **Application** layer — CQRS with MediatR. Depends on Domain only.

## Structure

```
Common/
├── Interfaces/     // IApplicationDbContext, IEmailService, ...
├── Models/         // Result<T>, PaginatedList<T>
├── Mappings/       // AutoMapper profiles
├── Behaviours/     // Logging, Validation (MediatR pipeline)
└── Exceptions/     // ValidationException → 400, ForbiddenException → 403

Features/
├── Products/       // Create/Update Commands, GetProducts/GetById Queries, DTOs
├── Users/          // Register, Login Commands, GetUser Query
├── Tenants/        // Create, Deactivate Commands, GetTenant/GetTenants Queries
└── Billing/        // GetCurrentPlan Query, UpgradePlan Command
```

## Pattern

```
Controller → MediatR → Command/Query → Handler → Repository → DB
                                     ↕
                          FluentValidation (Validator)
                          Logging (Behaviour)
                          Validation (Behaviour)
```

## Rules

- كل Use Case ليه Command/Query + Handler + Validator مستقلين
- الـ Handler مش بيستخدم الـ DbContext مباشرة — بيستخدم Repository
- الـ FluentValidation بيشتغل قبل الـ Handler (Behaviour Pipeline)
