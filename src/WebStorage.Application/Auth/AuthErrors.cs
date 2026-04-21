using WebStorage.Application.Common;

namespace WebStorage.Application.Auth;

public sealed class EmailAlreadyRegisteredException()
    : AppErrorException("An account with this email already exists.")
{
    public override int StatusCode => 409;

    public override string Title => "Email already registered";

    public override string? ErrorCode => ErrorCodes.Auth.EmailTaken;
}

public sealed class RegistrationFailedException()
    : AppErrorException("Registration could not be completed.")
{
    public override int StatusCode => 400;

    public override string Title => "Registration failed";

    public override string? ErrorCode => ErrorCodes.Auth.RegistrationFailed;
}

public sealed class InvalidCredentialsException()
    : AppErrorException("Invalid email or password.")
{
    public override int StatusCode => 401;

    public override string Title => "Invalid credentials";

    public override string? ErrorCode => ErrorCodes.Auth.InvalidCredentials;
}

public sealed class InvalidRefreshTokenException()
    : AppErrorException("The refresh token is invalid or has expired.")
{
    public override int StatusCode => 401;

    public override string Title => "Unauthorized";

    public override string? ErrorCode => ErrorCodes.Auth.InvalidRefreshToken;
}

public sealed class RefreshTokenMissingException()
    : AppErrorException("No refresh token was provided.")
{
    public override int StatusCode => 401;

    public override string Title => "Unauthorized";

    public override string? ErrorCode => ErrorCodes.Auth.RefreshTokenMissing;
}
