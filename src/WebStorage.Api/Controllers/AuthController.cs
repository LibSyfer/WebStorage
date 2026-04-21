using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebStorage.Application.Auth;
using WebStorage.Infrastructure.Options;

namespace WebStorage.Api.Controllers;

/// <summary>
/// Контроллер аутентификации и авторизации.
/// Предоставляет endpoints для регистрации, входа, обновления токена и выхода.
/// </summary>
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

    /// <summary>
    /// Регистрирует нового пользователя в системе.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    /// 
    ///     POST /api/auth/register
    ///     {
    ///         "email": "user@example.com",
    ///         "password": "SecurePassword123!"
    ///     }
    /// 
    /// Refresh токен автоматически сохраняется в HttpOnly cookie.
    /// </remarks>
    /// <param name="request">Данные для регистрации (email и пароль)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Access токен и информация пользователя</returns>
    /// <response code="200">Успешная регистрация, возвращен access токен</response>
    /// <response code="400">Некорректные данные или пользователь уже существует</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);

        SetRefreshCookie(result.RefreshToken, result.SessionExpiresAtUtc);

        return Ok(result.Auth);
    }

    /// <summary>
    /// Входит в систему с учётными данными пользователя.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    /// 
    ///     POST /api/auth/login
    ///     {
    ///         "email": "user@example.com",
    ///         "password": "SecurePassword123!"
    ///     }
    /// 
    /// Refresh токен автоматически сохраняется в HttpOnly cookie (более безопасно).
    /// Access токен возвращается в теле ответа и используется для других запросов.
    /// </remarks>
    /// <param name="request">Email и пароль пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Access токен с информацией пользователя</returns>
    /// <response code="200">Успешный вход, возвращен access токен</response>
    /// <response code="401">Некорректные учётные данные</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);

        SetRefreshCookie(result.RefreshToken, result.SessionExpiresAtUtc);

        return Ok(result.Auth);
    }

    /// <summary>
    /// Обновляет access токен используя refresh токен из cookie.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    /// 
    ///     POST /api/auth/refresh
    /// 
    /// Refresh токен берётся автоматически из cookie (HttpOnly).
    /// Новый access токен возвращается в теле ответа.
    /// Новый refresh токен сохраняется в cookie.
    /// </remarks>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Новый access токен</returns>
    /// <response code="200">Успешное обновление токена</response>
    /// <response code="401">Refresh токен отсутствует или невалиден</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Выходит из системы (логаут пользователя).
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    /// 
    ///     POST /api/auth/logout
    ///     Authorization: Bearer {accessToken}
    /// 
    /// Refresh токен удаляется из cookie.
    /// </remarks>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Никакого содержимого (204 No Content)</returns>
    /// <response code="204">Успешный выход, токен обновления очищен</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
