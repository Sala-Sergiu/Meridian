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
            _logger.LogWarning(
                validationException,
                "Validation failed for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteProblemAsync(context, BuildValidationProblem(context, validationException));
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteProblemAsync(context, BuildUnexpectedProblem(context));
        }
    }

    private static ValidationProblemDetails BuildValidationProblem(HttpContext context, ValidationException exception)
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

        AddCorrelationId(context, problem);
        return problem;
    }

    private static ProblemDetails BuildUnexpectedProblem(HttpContext context)
    {
        // No detail / stack trace leaked to the caller.
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://httpstatuses.io/500",
            Instance = context.Request.Path
        };

        AddCorrelationId(context, problem);
        return problem;
    }

    private static void AddCorrelationId(HttpContext context, ProblemDetails problem)
    {
        if (context.Items.TryGetValue(CorrelationIdMiddleware.ItemsKey, out var value) && value is string correlationId)
        {
            problem.Extensions["correlationId"] = correlationId;
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, ProblemDetails problem)
    {
        if (context.Response.HasStarted)
        {
            // Cannot rewrite a response that is already on the wire.
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = ProblemContentType;

        // Serialize with the runtime type so ValidationProblemDetails.Errors is included.
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, problem.GetType(), SerializerOptions));
    }
}
