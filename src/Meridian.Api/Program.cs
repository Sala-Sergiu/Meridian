// Composition root for the Meridian API.
// This is the ONLY place that may reference Meridian.Dal (for DI registration).

var builder = WebApplication.CreateBuilder(args);

// TODO: Serilog (structured logging) bootstrap.
// TODO: builder.Services.AddDal(builder.Configuration);  // DbContext, repositories, caching decorators.
// TODO: builder.Services.AddBll();                       // services, validators, Mapster, query pipeline.
// TODO: JWT authentication + policy-based authorization.
// TODO: ProblemDetails + health checks.
// TODO: Swagger/OpenAPI with JWT support.

builder.Services.AddControllers();

var app = builder.Build();

// TODO: correlation-id + exception-handling middleware.
// TODO: authentication / authorization.
// TODO: map health-check endpoint and Swagger UI.

app.MapControllers();

app.Run();
