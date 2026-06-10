using MediatR;
using Acme.SaaS.Application.Common.Interfaces;
using Acme.SaaS.Domain.Entities;
using Acme.SaaS.Domain.Interfaces;

namespace Acme.SaaS.Application.Features.Users;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly IRepository<User> _repository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IRepository<User> repository, IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsActive = true,
            Role = "Member",
            TenantId = request.TenantId
        };

        _repository.Add(user);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.Id, "User registered successfully");
    }
}
