using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebStorage.Application.Auth;
using WebStorage.Infrastructure.Options;

namespace WebStorage.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class AuthController(
    IAuthService authService,
    IOptions<AuthSessionOptions> authSessionOptions,
    IHostEnvironment hostEnvironment) : ControllerBase
{
    private readonly AuthSessionOptions _authSessionOptions = authSessionOptions.Value;
    private readonly bool _isDevelopment = hostEnvironment.IsDevelopment();

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        if (result is null)
            return Conflict();

        SetRefreshCookie(result.RefreshToken, result.SessionExpiresAtUtc);

        return Ok(result.Auth);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        if (result is null)
            return Unauthorized();

        SetRefreshCookie(result.RefreshToken, result.SessionExpiresAtUtc);

        return Ok(result.Auth);
    }

    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(_authSessionOptions.CookieName, out var refreshToken))
            return Unauthorized();

        var result = await authService.RefreshAsync(refreshToken, cancellationToken);
        if (result is null)
        {
            ClearRefreshCookie();
            return Unauthorized();
        }

        SetRefreshCookie(result.RefreshToken, result.SessionExpiresAtUtc);

        return Ok(result.Auth);
    }

    private void SetRefreshCookie(string refreshToken, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(_authSessionOptions.CookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_isDevelopment,
            SameSite = SameSiteMode.Lax,
            Expires = expiresAtUtc,
            Path = "/api/auth",
            IsEssential = true
        });
    }

    private void ClearRefreshCookie()
    {
        Response.Cookies.Delete(_authSessionOptions.CookieName, new CookieOptions
        {
            Secure = !_isDevelopment,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
        });
    }
}
