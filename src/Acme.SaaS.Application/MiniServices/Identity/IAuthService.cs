using Acme.SaaS.Application.Common.DTOs;

namespace Acme.SaaS.Application.MiniServices.Identity;

public interface IAuthService
{
    Task<ApiResponse<AuthResult>> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<ApiResponse<AuthResult>> RegisterAsync(RegisterRequest request, CancellationToken ct);
}

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string Role);
public record AuthResult(string Token, string Email, string Role);
