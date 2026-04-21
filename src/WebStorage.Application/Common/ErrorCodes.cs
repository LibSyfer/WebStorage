namespace WebStorage.Application.Common;

public static class ErrorCodes
{
    public static class Common
    {
        public const string Internal = "common.internal";
        public const string Unauthorized = "common.unauthorized";
        public const string Forbidden = "common.forbidden";
        public const string NotFound = "common.not_found";
    }

    public static class Auth
    {
        public const string EmailTaken = "auth.email_taken";
        public const string RegistrationFailed = "auth.registration_failed";
        public const string InvalidCredentials = "auth.invalid_credentials";
        public const string InvalidRefreshToken = "auth.invalid_refresh_token";
        public const string RefreshTokenMissing = "auth.refresh_token_missing";
    }

    public static class Storage
    {
        public const string QuotaExceeded = "storage.quota_exceeded";
        public const string FileNotFound = "storage.file_not_found";
        public const string InvalidKey = "storage.invalid_key";
        public const string BlobMissing = "storage.blob_missing";
    }

    public static class Validation
    {
        public const string BadRequest = "validation.bad_request";
    }
}
