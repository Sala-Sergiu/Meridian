using System.Security.Claims;
using System.Text;
using Meridian.Api.Authorization;
using Meridian.Api.Configuration;
using Meridian.Api.Filters;
using Meridian.Api.Middleware;
using Meridian.Api.Security;
using Meridian.Bll.Security;
using Meridian.Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace Meridian.Api;

// Composition helpers for the Api layer: token service, authentication/JWT,
// authorization policies, request middleware, controllers and Swagger.
public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");

        // JWT signing implementation lives in the Api layer.
        services.AddScoped<ITokenService, TokenService>();

        AddAuthentication(services, jwtOptions);
        AddAuthorization(services);

        services.AddTransient<CorrelationIdMiddleware>();
        services.AddTransient<ExceptionHandlingMiddleware>();

        services.AddControllers(options => options.Filters.Add<ValidationFilter>());

        AddSwagger(services);

        return services;
    }

    private static void AddAuthentication(IServiceCollection services, JwtOptions jwtOptions)
    {
        services
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
    }

    private static void AddAuthorization(IServiceCollection services)
    {
        // Resource/role aware policies (not flat role gates).
        services.AddScoped<IAuthorizationHandler, BoardOwnerAuthorizationHandler>();
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.CanRead, policy => policy.RequireAuthenticatedUser())
            .AddPolicy(Policies.HrWrite, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(nameof(Role.HR)))
            .AddPolicy(Policies.HrOrManagerRead, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(nameof(Role.HR), nameof(Role.Manager)))
            .AddPolicy(Policies.BoardOwnerWrite, policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new BoardOwnerRequirement()));
    }

    private static void AddSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
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
    }
}
