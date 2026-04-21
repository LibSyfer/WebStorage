using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebStorage.Application.Auth;
using WebStorage.Infrastructure.Options;

namespace WebStorage.Api.Controllers;

[Route("api/auth")]
[ApiController]
[AllowAnonymous]
public sealed class AuthController(
    IAuthService authService,
    IOptions<AuthSessionOptions> authSessionOptions,
    IHostEnvironment hostEnvironment) : ControllerBase
{
    private readonly AuthSessionOptions _authSessionOptions = authSessionOptions.Value;
    private readonly bool _isDevelopment = hostEnvironment.IsDevelopment();

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);

        SetRefreshCookie(result.RefreshToken, result.SessionExpiresAtUtc);

        return Ok(result.Auth);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);

        SetRefreshCookie(result.RefreshToken, result.SessionExpiresAtUtc);

        return Ok(result.Auth);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(_authSessionOptions.CookieName, out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
            throw new RefreshTokenMissingException();

        try
        {
            var result = await authService.RefreshAsync(refreshToken, cancellationToken);
            SetRefreshCookie(result.RefreshToken, result.SessionExpiresAtUtc);
            return Ok(result.Auth);
        }
        catch (InvalidRefreshTokenException)
        {
            ClearRefreshCookie();
            throw;
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult<AuthResponse>> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(_authSessionOptions.CookieName, out var refreshToken))
            await authService.LogoutAsync(refreshToken, cancellationToken);

        ClearRefreshCookie();

        return NoContent();
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
