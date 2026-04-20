namespace WebStorage.Infrastructure.Options;

public sealed class AuthSessionOptions
{
    public const string SectionName = "AuthSession";

    public string CookieName { get; set; } = "refresh_token";
    public int RefreshTokenExpirationDays { get; set; } = 14;
    public int TokenSizeBytes { get; set; } = 64;
}
