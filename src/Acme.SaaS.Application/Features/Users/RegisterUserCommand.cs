using MediatR;

namespace Acme.SaaS.Application.Features.Users;

public record RegisterUserCommand(
    string Email,
    string Password,
    string Name,
    Guid TenantId
) : IRequest<Result<Guid>>;
