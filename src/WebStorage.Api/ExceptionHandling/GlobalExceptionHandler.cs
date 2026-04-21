using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebStorage.Application.Common;

namespace WebStorage.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            AppErrorException app => CreateProblem(
                context,
                app.StatusCode,
                app.Title,
                app.Message,
                app.ErrorCode),

            ArgumentException => CreateProblem(
                context,
                400,
                "Bad request",
                exception.Message,
                ErrorCodes.Validation.BadRequest),

            UnauthorizedAccessException => CreateProblem(
                context,
                401,
                "Unauthorized",
                "Authentication is required.",
                ErrorCodes.Common.Unauthorized),

            FileNotFoundException => CreateProblem(
                context,
                404,
                "File not found",
                "The requested file was not found.",
                ErrorCodes.Storage.BlobMissing),

            OperationCanceledException => null,

            _ => CreateProblem(
                context,
                500,
                "Internal server error",
                "An unexpected error occurred.",
                ErrorCodes.Common.Internal)
        };

        Log(exception);

        if (problem is null)
            return true;

        context.Response.StatusCode = problem.Status ?? 500;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problem
        });

        return true;

    }

    private static ProblemDetails CreateProblem(
        HttpContext context,
        int status,
        string title,
        string detail,
        string? code)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{status}"
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        if (!string.IsNullOrEmpty(code))
            problem.Extensions["code"] = code;

        return problem;
    }

    private void Log(Exception ex)
    {
        switch (ex)
        {
            case AppErrorException:
            case ArgumentException:
                logger.LogWarning(ex, ex.Message);
                break;

            case OperationCanceledException:
                logger.LogInformation("Request was cancelled");
                break;

            default:
                logger.LogError(ex, "Unhandled exception");
                break;
        }
    }
}
