using Meridian.Api.Middleware;
using Microsoft.AspNetCore.Http;

namespace Meridian.UnitTests.Middleware;

public class CorrelationIdMiddlewareTests
{
    private static async Task<HttpContext> InvokeAsync(Action<HttpContext>? arrange = null)
    {
        var context = new DefaultHttpContext();
        arrange?.Invoke(context);

        var middleware = new CorrelationIdMiddleware();
        await middleware.InvokeAsync(context, _ => Task.CompletedTask);

        return context;
    }

    [Fact]
    public async Task GeneratesCorrelationId_WhenHeaderAbsent()
    {
        var context = await InvokeAsync();

        var header = context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString();

        Assert.False(string.IsNullOrWhiteSpace(header));
        Assert.True(Guid.TryParse(header, out _));
        Assert.Equal(header, context.Items[CorrelationIdMiddleware.ItemsKey]);
    }

    [Fact]
    public async Task ReusesCorrelationId_WhenHeaderPresent()
    {
        const string incoming = "incoming-correlation-id";

        var context = await InvokeAsync(c => c.Request.Headers[CorrelationIdMiddleware.HeaderName] = incoming);

        Assert.Equal(incoming, context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
        Assert.Equal(incoming, context.Items[CorrelationIdMiddleware.ItemsKey]);
    }
}
