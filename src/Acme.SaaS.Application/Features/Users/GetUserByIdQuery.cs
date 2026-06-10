using MediatR;

namespace Acme.SaaS.Application.Features.Users;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
