using Acme.SaaS.Application.Common.Interfaces;

namespace Acme.SaaS.Infrastructure.Services.Identity;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
