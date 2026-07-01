using System.Security.Claims;
using System.Text;
using Meridian.Api.Authorization;
using Meridian.Api.Configuration;
using Meridian.Api.Filters;
using Meridian.Api.Middleware;
using Meridian.Api.Security;
using Meridian.Bll;
using Meridian.Bll.Security;
using Meridian.Dal;
using Meridian.Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Structured logging via Serilog, configured from appsettings ("Serilog" section).
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Layers (Dal referenced only here, in the composition root).
builder.Services.AddDal(builder.Configuration);
builder.Services.AddBll();

// JWT signing implementation lives in the Api layer.
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");
builder.Services.AddScoped<ITokenService, TokenService>();

// Authentication — validate JWTs issued by this API.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            NameClaimType = "name",
            RoleClaimType = ClaimTypes.Role
        };
    });

// Authorization — resource/role aware policies (not flat role gates).
builder.Services.AddScoped<IAuthorizationHandler, BoardOwnerAuthorizationHandler>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.CanRead, policy => policy.RequireAuthenticatedUser())
    .AddPolicy(Policies.HrWrite, policy => policy
        .RequireAuthenticatedUser()
        .RequireRole(nameof(Role.HR)))
    .AddPolicy(Policies.BoardOwnerWrite, policy => policy
        .RequireAuthenticatedUser()
        .AddRequirements(new BoardOwnerRequirement()));

builder.Services.AddTransient<CorrelationIdMiddleware>();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());

// Swagger with JWT bearer support (Authorize button).
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT returned by POST /api/auth/login."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            new List<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Correlation id first, so every subsequent log line (including the request
// summary) carries it.
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

// Catch-all: turns any unhandled exception into an RFC 7807 ProblemDetails.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
