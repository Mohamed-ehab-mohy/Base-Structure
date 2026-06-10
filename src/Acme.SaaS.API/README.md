# Acme.SaaS.API

The **Presentation** layer — ASP.NET Core Web API entry point.

## Structure

- **Controllers**: Thin — call MediatR, return responses
- **Middlewares**: ExceptionHandling, RequestLogging, TenantResolution
- **Filters**: SwaggerDefaultValues
- **Models**: ApiResponse, PaginatedResponse
- **Resources**: Arabic/English localization
- **Extensions**: ServiceCollection + ApplicationBuilder registrations

## Pipeline

```
HTTP → ExceptionMiddleware → RequestLoggingMiddleware → TenantResolutionMiddleware
→ JWT Auth → Controller → MediatR → Handler → Repository → DB
→ Response (ApiResponse wrapper)
```

## Configuration

- `appsettings.json` — DB connection, JWT, Tenant mode, SMTP, Storage
- Swagger available at `/swagger`
