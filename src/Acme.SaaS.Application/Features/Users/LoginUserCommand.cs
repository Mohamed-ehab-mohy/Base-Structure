using MediatR;

namespace Acme.SaaS.Application.Features.Users;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<Result<string>>;
