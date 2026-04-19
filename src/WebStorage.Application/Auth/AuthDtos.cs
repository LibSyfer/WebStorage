namespace WebStorage.Application.Auth;

public sealed record RegisterRequest(string Email, string Password);
public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(string AccessToken, string Email, IReadOnlyList<string> Roles, DateTime ExpiresAtUtc);

public sealed record AuthSessionResult(AuthResponse Auth, string RefreshToken);

