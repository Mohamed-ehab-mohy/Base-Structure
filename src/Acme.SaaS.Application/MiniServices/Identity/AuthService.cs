using Acme.SaaS.Application.Common.DTOs;
using Acme.SaaS.Application.Common.Interfaces;
using Acme.SaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Acme.SaaS.Application.MiniServices.Identity;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IApplicationDbContext context, ITenantProvider tenantProvider, IPasswordHasher passwordHasher)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<AuthResult>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.TenantId == _tenantProvider.GetTenantId(), ct);

        if (user is null || !user.IsActive || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return ApiResponse<AuthResult>.Fail("Invalid credentials.");

        return ApiResponse<AuthResult>.Ok(new AuthResult(
            $"jwt-placeholder-{user.Id}", user.Email, user.Role));
    }

    public async Task<ApiResponse<AuthResult>> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var existing = await _context.Users
            .AnyAsync(u => u.Email == request.Email && u.TenantId == _tenantProvider.GetTenantId(), ct);
        if (existing)
            return ApiResponse<AuthResult>.Fail("Email already registered.");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role,
            TenantId = _tenantProvider.GetTenantId()
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        return ApiResponse<AuthResult>.Ok(new AuthResult(
            $"jwt-placeholder-{user.Id}", user.Email, user.Role), "Registration successful.");
    }
}
