namespace WebStorage.Application.Auth;

public sealed record RegisterRequest(string Email, string Password);
public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(string AccessToken, string Email, DateTime ExpiresAtUtc);

public sealed record AuthSessionResult(AuthResponse Auth, string RefreshToken);