using Acme.SaaS.Application.MiniServices.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Acme.SaaS.API.Controllers;

public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct) =>
        ToActionResult(await _authService.LoginAsync(request, ct));

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct) =>
        ToActionResult(await _authService.RegisterAsync(request, ct));
}
