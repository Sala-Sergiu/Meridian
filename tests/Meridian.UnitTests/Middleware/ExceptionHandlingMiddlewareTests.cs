using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Meridian.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Meridian.UnitTests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private static ExceptionHandlingMiddleware CreateMiddleware()
        => new(NullLogger<ExceptionHandlingMiddleware>.Instance);

    private static JsonElement ReadBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var document = JsonDocument.Parse(context.Response.Body);
        return document.RootElement.Clone();
    }

    [Fact]
    public async Task UnhandledException_ProducesProblemDetails_WithCorrelationId()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var correlation = new CorrelationIdMiddleware();
        var exceptionHandler = CreateMiddleware();

        // Real pipeline shape: correlation id -> exception handler -> throwing endpoint.
        RequestDelegate endpoint = _ => throw new InvalidOperationException("boom");
        await correlation.InvokeAsync(context, ctx => exceptionHandler.InvokeAsync(ctx, endpoint));

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        var body = ReadBody(context);
        Assert.Equal(500, body.GetProperty("status").GetInt32());

        var correlationId = body.GetProperty("correlationId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(correlationId));

        // ProblemDetails correlationId matches the response header.
        Assert.Equal(context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString(), correlationId);

        // No stack trace / detail leaked.
        Assert.False(body.TryGetProperty("detail", out _));
    }

    [Fact]
    public async Task ValidationException_ProducesBadRequestProblemDetails_WithErrors()
    {
        var context = new DefaultHttpContext();
        context.Items[CorrelationIdMiddleware.ItemsKey] = "cid-123";
        context.Response.Body = new MemoryStream();

        var failures = new[] { new ValidationFailure("Email", "'Email' must not be empty.") };
        RequestDelegate endpoint = _ => throw new ValidationException(failures);

        await CreateMiddleware().InvokeAsync(context, endpoint);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        var body = ReadBody(context);
        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.Equal("cid-123", body.GetProperty("correlationId").GetString());
        Assert.True(body.GetProperty("errors").TryGetProperty("Email", out _));
    }
}
