# Migration Changelog

## Architecture Evolution

### Phase 1 — Initial: Mini-Services Pattern

```
MiniServices/
├── Tenants/       // ITenantService + TenantService
├── Identity/      // IAuthService + AuthService
├── Billing/       // IBillingService + BillingService
└── Products/      // IProductService + ProductService
```

### Phase 2 — Current: CQRS + MediatR Pattern

```
Features/
├── Products/
│   ├── Commands/CreateProduct/     // Command + Handler + Validator
│   ├── Commands/UpdateProduct/     // Command + Handler + Validator
│   ├── Queries/GetProductById/     // Query + Handler
│   ├── Queries/GetProducts/        // Query + Handler
│   └── DTOs/                       // ProductDto, CreateProductDto
├── Users/          // RegisterUserCommand, LoginUserCommand, GetUserByIdQuery
├── Tenants/        // CreateTenantCommand, DeactivateTenantCommand, Queries
└── Billing/        // GetCurrentPlanQuery, UpgradePlanCommand
```

## Key Changes

### 1. CQRS Pattern (بدلاً من Mini-Services)

**Before:**
```csharp
// Service واحد لكل Feature
public interface IProductService
{
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto> GetProductByIdAsync(Guid id);
}
public class ProductService : IProductService { ... }
```

**After:**
```csharp
// كل Use Case في Command/Query منفصل
public record CreateProductCommand(string Name, decimal Price) : IRequest<Result<Guid>>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // Business logic هنا
    }
}
```

### 2. إضافة Domain Events

- `OrderCreatedEvent` — بيحصل لما Order يتعمل
- `UserRegisteredEvent` — بيحصل لما User يتسجل

### 3. إضافة Value Objects

- `Money` — Amount + Currency مع عمليات Add/Subtract
- `Address` — Street, City, Country, PostalCode

### 4. نقل الـ Interfaces

- `IRepository<T>` و `IUnitOfWork` اتنقلوا من Application → Domain
- عشان Domain يفضل هو المسؤول عن تعريف العقود الأساسية

### 5. MediatR Pipeline Behaviours

- `LoggingBehaviour` — يسجل كل Request
- `ValidationBehaviour` — يشغل FluentValidation قبل الـ Handler

### 6. FluentValidation لكل Command

كل Command ليه Validator منفصل:

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

### 7. Result Pattern

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Message { get; }
    public string[]? Errors { get; }
}
```

### 8. إضافات Infrastructure

- `EmailService` — تجريد لإرسال الإيميلات
- `FileStorageService` — رفع وتبادل الملفات
- `PaymentGatewayService` — معالجة المدفوعات
- `ApplicationUser` / `ApplicationRole` — Identity Models

### 9. إضافات API

- `Models/ApiResponse.cs` — Response موحد
- `Models/PaginatedResponse.cs` — Pagination Response
- `Resources/` — Localization (ar-EG, en-US)
- `Dockerfile` + `docker-compose.yml` — Containerization

## Dependency Graph

```
API → Infrastructure → Application → Domain
     ↘ Infrastructure implements Application interfaces ↗
```

## Technologies

| Component | Technology |
|---|---|
| .NET Version | 10.0 |
| ORM | Entity Framework Core 10.0 |
| Database | SQL Server |
| CQRS | MediatR 12 |
| Validation | FluentValidation 12 |
| Mapping | AutoMapper 12 |
| Auth | JWT Bearer |
| Password Hashing | BCrypt |
| Testing | xUnit |
| API Docs | Swagger / OpenAPI |
