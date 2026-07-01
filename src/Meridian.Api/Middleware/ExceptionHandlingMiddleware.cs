namespace Meridian.Api.Middleware;

// Catch-all middleware → returns ProblemDetails (RFC 7807),
// including the correlation id in every error response.
public class ExceptionHandlingMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // TODO: try/catch around next(context); map exceptions to ProblemDetails per spec.
        throw new NotImplementedException();
    }
}
