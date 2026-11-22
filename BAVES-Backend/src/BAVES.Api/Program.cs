using BAVES.Api.Shared.Exceptions;
using BAVES.Api.Modules;
using BAVES.Api.Modules.Identity;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BA-VES API",
        Version = "v1",
        Description = "Backend modular monolith for the BA-VES platform."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT",
        Description = "Enter the access token returned by register, login or refresh."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        }] = []
    });

    var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    options.AddPolicy("DesktopAndWebClients", policy =>
    {
        // Cho phép browser đọc metadata phân trang từ response header.
        string[] paginationHeaders =
        [
            "X-Total-Count",
            "X-Page",
            "X-Page-Size",
            "X-Total-Pages"
        ];

        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders(paginationHeaders);
            return;
        }

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders(paginationHeaders);
    });
});

builder.Services.AddBavesModules(builder.Configuration);
builder.Services.AddIdentityAuthentication(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "BA-VES API v1");
    options.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseCors("DesktopAndWebClients");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithTags("System")
    .WithName("HealthCheck");

if (builder.Configuration.GetValue<bool>("Database:EnsureCreated"))
{
    throw new InvalidOperationException(
        "Database:EnsureCreated is unsupported with module-owned schemas. " +
        "Use per-module migrations or the reviewed initial schema script.");
}

app.Run();

public partial class Program;
