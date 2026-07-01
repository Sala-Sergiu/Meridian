using Serilog.Context;

namespace Meridian.Api.Middleware;

// First middleware in the pipeline. Establishes a correlation id for the request:
// reuses an incoming X-Correlation-ID header when present, otherwise generates a
// new GUID. Pushes it into the Serilog LogContext (so every log line for this
// request carries it), stashes it in HttpContext.Items for downstream code
// (e.g. the exception handler), and writes it back on the response.
public class CorrelationIdMiddleware : IMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    public const string ItemsKey = "CorrelationId";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = ResolveCorrelationId(context);

        context.Items[ItemsKey] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty(ItemsKey, correlationId))
        {
            await next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var incoming)
            && !string.IsNullOrWhiteSpace(incoming))
        {
            return incoming.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}
