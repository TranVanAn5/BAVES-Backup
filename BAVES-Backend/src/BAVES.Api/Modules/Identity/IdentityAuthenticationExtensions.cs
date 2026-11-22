using System.Text;
using System.Threading.RateLimiting;
using BAVES.Api.Modules.Identity.Models;
using BAVES.Api.Modules.Identity.Repositories;
using BAVES.Api.Modules.Identity.Repositories.Interfaces;
using BAVES.Api.Modules.Identity.Services;
using BAVES.Api.Modules.Identity.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BAVES.Api.Modules.Identity;

public static class IdentityAuthenticationExtensions
{
    public const string AuthRateLimitPolicy = "identity-auth";

    public static IServiceCollection AddIdentityAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey) &&
                Encoding.UTF8.GetByteCount(options.SigningKey) >= 32,
                "Jwt:SigningKey must contain at least 32 UTF-8 bytes. Configure it using user secrets or an environment variable.")
            .ValidateOnStart();
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, settings) =>
            {
                var jwt = settings.Value;
                options.MapInboundClaims = false;
                options.IncludeErrorDetails = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, ValidIssuer = jwt.Issuer,
                    ValidateAudience = true, ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    RequireSignedTokens = true, ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ValidateLifetime = true, RequireExpirationTime = true, ClockSkew = TimeSpan.Zero,
                    NameClaimType = "name", RoleClaimType = JwtTokenService.RoleClaim
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var tokens = context.HttpContext.RequestServices.GetRequiredService<IJwtTokenService>();
                        if (context.Principal is null ||
                            !await tokens.ValidateSessionAsync(context.Principal, context.HttpContext.RequestAborted))
                            context.Fail("The session is no longer valid.");
                    },
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.Headers.CacheControl = "no-store";
                        context.Response.Headers.WWWAuthenticate = "Bearer";
                        await context.Response.WriteAsJsonAsync(new ProblemDetails
                        {
                            Status = StatusCodes.Status401Unauthorized, Title = "Unauthorized",
                            Detail = "A valid access token is required.", Instance = context.Request.Path
                        }, context.HttpContext.RequestAborted);
                    }
                };
            });
        services.AddAuthorization(options =>
        {
            options.AddPolicy(BAVES.Api.Shared.Authorization.Policies.AnyAuthenticated,
                policy => policy.RequireAuthenticatedUser());
            options.AddPolicy(BAVES.Api.Shared.Authorization.Policies.LearnerOnly,
                policy => policy.RequireRole("LEARNER"));
            options.AddPolicy(BAVES.Api.Shared.Authorization.Policies.InstructorOrAdmin,
                policy => policy.RequireRole("INSTRUCTOR", "ADMIN"));
            options.AddPolicy(BAVES.Api.Shared.Authorization.Policies.ExpertOrInstructor,
                policy => policy.RequireRole("EXPERT", "BAExpert", "BAEXPERT", "INSTRUCTOR", "ADMIN"));
        });
        services.AddRateLimiter(options =>
        {
            options.AddPolicy(AuthRateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 60, Window = TimeSpan.FromMinutes(1), QueueLimit = 0,
                    AutoReplenishment = true
                }));
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.Headers.CacheControl = "no-store";
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds)
                        .ToString(System.Globalization.CultureInfo.InvariantCulture);
                await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests, Title = "Too many requests",
                    Detail = "Please wait before trying again.", Instance = context.HttpContext.Request.Path
                }, cancellationToken);
            };
        });
        return services;
    }
}
