namespace WebStorage.Application.Auth;

public interface IJwtTokenGenerator
{
    string CreateAccessToken(string userId, string email, IReadOnlyList<string> roles, DateTime expiresAtUtc);
}
