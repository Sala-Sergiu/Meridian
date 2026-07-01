using System.Text;
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

    private static string ReadRawBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEnd();
    }

    [Fact]
    public async Task UnhandledException_ProducesProblemDetails_WithCorrelationId()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var correlation = new CorrelationIdMiddleware();
        var exceptionHandler = CreateMiddleware();

        // Real pipeline shape: correlation id -> exception handler -> throwing endpoint.
        RequestDelegate endpoint = _ => throw new InvalidOperationException("super-secret-internal-detail");
        await correlation.InvokeAsync(context, ctx => exceptionHandler.InvokeAsync(ctx, endpoint));

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        var raw = ReadRawBody(context);
        using var document = JsonDocument.Parse(raw);
        var body = document.RootElement;

        Assert.Equal(500, body.GetProperty("status").GetInt32());

        var correlationId = body.GetProperty("correlationId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(correlationId));

        // correlationId matches the X-Correlation-ID response header.
        Assert.Equal(context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString(), correlationId);
    }

    [Fact]
    public async Task UnhandledException_DoesNotLeakExceptionMessageOrStackTrace()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var correlation = new CorrelationIdMiddleware();
        RequestDelegate endpoint = _ => throw new InvalidOperationException("super-secret-internal-detail");
        await correlation.InvokeAsync(context, ctx => CreateMiddleware().InvokeAsync(ctx, endpoint));

        var raw = ReadRawBody(context);

        Assert.DoesNotContain("super-secret-internal-detail", raw);
        Assert.DoesNotContain("InvalidOperationException", raw);
        Assert.DoesNotContain("StackTrace", raw, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidationException_ProducesBadRequestProblemDetails_WithErrors()
    {
        var context = new DefaultHttpContext();
        // Correlation id is read from the X-Correlation-ID response header set upstream.
        context.Response.Headers[CorrelationIdMiddleware.HeaderName] = "cid-123";
        context.Response.Body = new MemoryStream();

        var failures = new[] { new ValidationFailure("Email", "'Email' must not be empty.") };
        RequestDelegate endpoint = _ => throw new ValidationException(failures);

        await CreateMiddleware().InvokeAsync(context, endpoint);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        using var document = JsonDocument.Parse(ReadRawBody(context));
        var body = document.RootElement;

        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.Equal("cid-123", body.GetProperty("correlationId").GetString());

        var emailErrors = body.GetProperty("errors").GetProperty("Email");
        Assert.Equal("'Email' must not be empty.", emailErrors[0].GetString());
    }
}
