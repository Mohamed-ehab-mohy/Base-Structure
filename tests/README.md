# Tests

xUnit-based test projects matching each layer.

| Project | Focus | Approach |
|---|---|---|
| `Domain.Tests` | Entities, Value Objects, Events | Pure unit tests |
| `Application.Tests` | Handlers, Validators, Behaviours | Unit + mocks |
| `Infrastructure.Tests` | Repositories, Interceptors, Services | Integration |
| `API.Tests` | Controllers, Middlewares, Endpoints | E2E (integration test server) |

## Running Tests

```bash
dotnet test
```
