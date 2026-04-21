namespace WebStorage.Application.Auth;

public interface IAuthService
{
    Task<AuthSessionResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthSessionResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthSessionResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
