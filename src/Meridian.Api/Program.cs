using Meridian.Api;
using Meridian.Api.Middleware;
using Meridian.Bll;
using Meridian.Dal;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Structured logging via Serilog, configured from appsettings ("Serilog" section).
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// DI, one extension method per layer (Dal referenced only here, in the composition root).
builder.Services
    .AddDal(builder.Configuration)
    .AddBll()
    .AddApiServices(builder.Configuration);

var app = builder.Build();

// Middleware order: correlation id -> request logging -> exception handler ->
// authentication -> authorization -> endpoints. Correlation id is first so every
// response (Swagger included) and every log line carries it.
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        if (httpContext.Items.TryGetValue(CorrelationIdMiddleware.ItemsKey, out var correlationId)
            && correlationId is string value)
        {
            diagnosticContext.Set(CorrelationIdMiddleware.ItemsKey, value);
        }
    };
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
