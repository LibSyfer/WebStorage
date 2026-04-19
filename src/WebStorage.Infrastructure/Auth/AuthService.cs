using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebStorage.Application.Auth;
using WebStorage.Infrastructure.Data;
using WebStorage.Infrastructure.Identity;
using WebStorage.Infrastructure.Options;

namespace WebStorage.Infrastructure.Auth;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator jwtTokenGenerator,
    AppDbContext dbContext,
    IOptions<AuthSessionOptions> sessionOptions) : IAuthService
{
    private readonly AuthSessionOptions _sessionOptions = sessionOptions.Value;

    public async Task<AuthSessionResult?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existedUser = await userManager.FindByEmailAsync(request.Email);
        if (existedUser is not null)
            return null;

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return null;

        await userManager.AddToRoleAsync(user, RoleNames.User);

        return await BuildAuthSessionResultAsync(user, cancellationToken);
    }

    public async Task<AuthSessionResult?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user is null)
            return null;

        var valid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!valid)
            return null;

        return await BuildAuthSessionResultAsync(user, cancellationToken);
    }

    public async Task<AuthSessionResult?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(refreshToken);
        var session = await dbContext.RefreshSessions.FirstOrDefaultAsync(s => s.TokenHash == tokenHash, cancellationToken);
        if (session is null)
            return null;

        if (session.RevokedAtUtc is not null || session.ExpiresAtUtc <= DateTime.UtcNow)
            return null;

        var user = await userManager.FindByIdAsync(session.UserId);
        if (user is null)
            return null;

        var newSession = CreateSession(user.Id, out var rawRefreshToken);
        session.RevokedAtUtc = DateTime.UtcNow;

        dbContext.RefreshSessions.Add(newSession);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        var authResponse = await BuildAuthResponseAsync(user);
        return new AuthSessionResult(authResponse, rawRefreshToken);
    }

    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hashedToken = HashToken(refreshToken);
        var session = await dbContext.RefreshSessions.FirstOrDefaultAsync(s => s.TokenHash == hashedToken, cancellationToken);
        if (session is null || session.RevokedAtUtc is not null)
            return false;

        session.RevokedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user)
    {
        var userClaims = await BuildUserClaimsAsync(user);
        var accessToken = jwtTokenGenerator.CreateAccessToken(userClaims, out var expiresAtUtc);
        return new AuthResponse(accessToken, user.Email ?? user.UserName ?? string.Empty, expiresAtUtc);
    }

    private async Task<IReadOnlyList<Claim>> BuildUserClaimsAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.Id),
            new (JwtRegisteredClaimNames.Email, user.Email ?? user.UserName ?? string.Empty),
            new (ClaimTypes.NameIdentifier, user.Id)
        };

        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }

    private async Task<AuthSessionResult> BuildAuthSessionResultAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var authResponse = await BuildAuthResponseAsync(user);

        var session = CreateSession(user.Id, out var rawRefreshToken);
        dbContext.RefreshSessions.Add(session);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthSessionResult(authResponse, rawRefreshToken);
    }

    private RefreshSession CreateSession(string userId, out string rawRefreshToken)
    {
        rawRefreshToken = GenerateRefreshToken();
        var now = DateTime.UtcNow;
        var sessionId = Guid.NewGuid();

        return new RefreshSession
        {
            Id = sessionId,
            UserId = userId,
            TokenHash = HashToken(rawRefreshToken),
            ExpiresAtUtc = now.AddDays(_sessionOptions.RefreshTokenExpirationDays),
            CreatedAtUtc = now
        };
    }

    private string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(_sessionOptions.TokenSizeBytes);
        return Convert.ToBase64String(bytes);
    }

    private string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }
}
