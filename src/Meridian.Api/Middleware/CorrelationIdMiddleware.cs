namespace Meridian.Api.Middleware;

// Generates/propagates a correlation id per request and enriches Serilog logs with it.
public class CorrelationIdMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // TODO: read/generate correlation id, push to log context, set response header.
        throw new NotImplementedException();
    }
}
