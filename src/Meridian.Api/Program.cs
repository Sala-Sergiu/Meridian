using Meridian.Api;
using Meridian.Api.Middleware;
using Meridian.Bll;
using Meridian.Dal;
using Meridian.Dal.Persistence;
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

// Dev-only CORS for the Angular dev server (ng serve). Allows the bearer
// Authorization and X-Correlation-ID request headers and exposes the
// correlation id on responses so the frontend can read it. Production serves
// the client from the same origin (or gets its own explicit policy) — this
// policy is registered and applied in Development only.
const string devCorsPolicy = "DevClient";
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options => options.AddPolicy(devCorsPolicy, policy => policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders(CorrelationIdMiddleware.HeaderName)));
}

var app = builder.Build();

// Containerized runs (docker compose) apply migrations + HasData seed here;
// local dev applies them explicitly via `dotnet ef database update`.
if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsAtStartup"))
{
    await DatabaseInitializer.ApplyMigrationsAsync(app.Services);
}

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
    app.UseCors(devCorsPolicy);
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
