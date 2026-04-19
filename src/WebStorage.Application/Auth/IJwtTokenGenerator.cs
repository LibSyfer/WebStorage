using System.Security.Claims;

namespace WebStorage.Application.Auth;

public interface IJwtTokenGenerator
{
    string CreateAccessToken(IReadOnlyList<Claim> claims, out DateTime expiresAtUtc);
}
