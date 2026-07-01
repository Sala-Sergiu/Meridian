using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Middleware;

// Catch-all middleware: wraps the pipeline, logs any unhandled exception with the
// correlation id, and returns an RFC 7807 ProblemDetails (application/problem+json)
// carrying a "correlationId" so callers can match the error to the logs.
// FluentValidation failures map to 400; everything else to 500. Stack traces are
// never written to the response body.
public class ExceptionHandlingMiddleware : IMiddleware
{
    private const string ProblemContentType = "application/problem+json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException validationException)
        {
            var correlationId = GetCorrelationId(context);

            _logger.LogWarning(
                validationException,
                "Validation failed for {Method} {Path} (CorrelationId: {CorrelationId})",
                context.Request.Method,
                context.Request.Path,
                correlationId);

            await WriteProblemAsync(context, BuildValidationProblem(context, validationException, correlationId), correlationId);
        }
        catch (Exception exception)
        {
            var correlationId = GetCorrelationId(context);

            // Full exception (message + stack) goes to the log only — never to the response.
            _logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path} (CorrelationId: {CorrelationId})",
                context.Request.Method,
                context.Request.Path,
                correlationId);

            await WriteProblemAsync(context, BuildUnexpectedProblem(context, correlationId), correlationId);
        }
    }

    private static ValidationProblemDetails BuildValidationProblem(HttpContext context, ValidationException exception, string? correlationId)
    {
        var errors = exception.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray());

        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Type = "https://httpstatuses.io/400",
            Instance = context.Request.Path
        };

        AddCorrelationId(problem, correlationId);
        return problem;
    }

    private static ProblemDetails BuildUnexpectedProblem(HttpContext context, string? correlationId)
    {
        // Generic, safe message only — the real detail/stack trace stays in the log.
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://httpstatuses.io/500",
            Instance = context.Request.Path
        };

        AddCorrelationId(problem, correlationId);
        return problem;
    }

    private static void AddCorrelationId(ProblemDetails problem, string? correlationId)
    {
        if (correlationId is not null)
        {
            problem.Extensions["correlationId"] = correlationId;
        }
    }

    // Read the correlation id from the X-Correlation-ID response header set upstream
    // by the correlation id middleware, so the error response and the logs share it.
    private static string? GetCorrelationId(HttpContext context)
    {
        var value = context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static async Task WriteProblemAsync(HttpContext context, ProblemDetails problem, string? correlationId)
    {
        if (context.Response.HasStarted)
        {
            // Cannot rewrite a response that is already on the wire.
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = ProblemContentType;

        // Response.Clear() dropped the header set by the correlation middleware — re-apply it
        // so error responses still carry X-Correlation-ID.
        if (correlationId is not null)
        {
            context.Response.Headers[CorrelationIdMiddleware.HeaderName] = correlationId;
        }

        // Serialize with the runtime type so ValidationProblemDetails.Errors is included.
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, problem.GetType(), SerializerOptions));
    }
}
