using BAVES.Api.Modules.Analysis.Data;
using BAVES.Api.Modules.Artifact.Data;
using BAVES.Api.Modules.Assessment.Data;
using BAVES.Api.Modules.Commerce.Data;
using BAVES.Api.Modules.Content.Data;
using BAVES.Api.Modules.Content.Repositories;
using BAVES.Api.Modules.Content.Repositories.Interfaces;
using BAVES.Api.Modules.Identity.Data;
using BAVES.Api.Modules.Identity.Models.Entities;
using BAVES.Api.Modules.Planning.Data;
using BAVES.Api.Modules.Planning.Repositories;
using BAVES.Api.Modules.Planning.Repositories.Interfaces;
using BAVES.Api.Modules.Scenario.Data;
using BAVES.Api.Modules.Scenario.Repositories;
using BAVES.Api.Modules.Scenario.Repositories.Interfaces;
using BAVES.Api.Modules.Scenario.Services;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using BAVES.Api.Modules.Simulation.Data;
using BAVES.Api.Modules.Training.Data;
using BAVES.Api.Modules.Simulation.Repositories;
using BAVES.Api.Modules.Simulation.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BAVES.Api.Modules;

public static class DependencyInjection
{
    public static IServiceCollection AddBavesModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");

        AddModuleContext<IdentityDbContext>(services, connectionString, "identity");
        AddModuleContext<ContentDbContext>(services, connectionString, "content");
        AddModuleContext<TrainingDbContext>(services, connectionString, "training");
        AddModuleContext<ScenarioDbContext>(services, connectionString, "scenario");
        AddModuleContext<SimulationDbContext>(services, connectionString, "simulation");
        AddModuleContext<PlanningDbContext>(services, connectionString, "planning");
        AddModuleContext<AnalysisDbContext>(services, connectionString, "analysis");
        AddModuleContext<ArtifactDbContext>(services, connectionString, "artifact");
        AddModuleContext<AssessmentDbContext>(services, connectionString, "assessment");
        AddModuleContext<CommerceDbContext>(services, connectionString, "commerce");

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddSignInManager();

        services.AddScoped<IScenarioCatalogRepository, ScenarioCatalogRepository>();



        services.AddScoped<IScenarioSessionContextRepository, ScenarioSessionContextRepository>();

        services.AddScoped<IScenarioCatalogService, ScenarioCatalogService>();

        services.AddScoped<IContentReadRepository, ContentReadRepository>();

        services.AddScoped<BAVES.Api.Modules.Content.Services.Interfaces.IContentReadService,
            BAVES.Api.Modules.Content.Services.ContentReadService>();

        services.AddScoped<BAVES.Api.Modules.Scenario.Repositories.Interfaces.IScenarioAuthoringRepository,
            BAVES.Api.Modules.Scenario.Repositories.ScenarioAuthoringRepository>();

        services.AddScoped<BAVES.Api.Modules.Scenario.Services.Interfaces.IScenarioAuthoringService,
            BAVES.Api.Modules.Scenario.Services.ScenarioAuthoringService>();

        services.AddScoped<BAVES.Api.Modules.Simulation.Services.Interfaces.ISimulationSessionService,
            BAVES.Api.Modules.Simulation.Services.SimulationSessionService>();

        services.AddScoped<BAVES.Api.Modules.Simulation.Services.Interfaces.ICoachFeedbackService,
            BAVES.Api.Modules.Simulation.Services.CoachFeedbackService>();
        services.AddScoped<BAVES.Api.Modules.Simulation.Services.Interfaces.ISprintSimulationService,
            BAVES.Api.Modules.Simulation.Services.SprintSimulationService>();

        services.AddScoped<BAVES.Api.Modules.Planning.Services.Interfaces.IPlanningService,
            BAVES.Api.Modules.Planning.Services.PlanningService>();

        services.AddScoped<ISimulationSessionRepository, SimulationSessionRepository>();

        services.AddScoped<IPlanningRepository, PlanningRepository>();

        services.AddScoped<BAVES.Api.Modules.Analysis.Repositories.Interfaces.IAnalysisItemRepository,
            BAVES.Api.Modules.Analysis.Repositories.AnalysisItemRepository>();

        services.AddScoped<BAVES.Api.Modules.Analysis.Services.Interfaces.IAnalysisItemService,
            BAVES.Api.Modules.Analysis.Services.AnalysisItemService>();

        services.AddScoped<BAVES.Api.Modules.Artifact.Repositories.Interfaces.IArtifactRepository,
            BAVES.Api.Modules.Artifact.Repositories.ArtifactRepository>();

        services.AddScoped<BAVES.Api.Modules.Artifact.Services.Interfaces.IArtifactService,
            BAVES.Api.Modules.Artifact.Services.ArtifactService>();

        services.AddScoped<BAVES.Api.Modules.Assessment.Repositories.Interfaces.IAssessmentRepository,
            BAVES.Api.Modules.Assessment.Repositories.AssessmentRepository>();

        services.AddScoped<BAVES.Api.Modules.Assessment.Repositories.Interfaces.IBookingRepository,
            BAVES.Api.Modules.Assessment.Repositories.BookingRepository>();

        services.AddScoped<BAVES.Api.Modules.Assessment.Services.Interfaces.IAssessmentService,
            BAVES.Api.Modules.Assessment.Services.AssessmentService>();

        services.AddScoped<BAVES.Api.Modules.Assessment.Services.Interfaces.IBookingService,
            BAVES.Api.Modules.Assessment.Services.BookingService>();

        services.AddScoped<BAVES.Api.Modules.Training.Repositories.Interfaces.ICohortRepository,
            BAVES.Api.Modules.Training.Repositories.CohortRepository>();

        services.AddScoped<BAVES.Api.Modules.Training.Services.Interfaces.ICohortService,
            BAVES.Api.Modules.Training.Services.CohortService>();

        services.AddScoped<BAVES.Api.Modules.Training.Repositories.Interfaces.IAssignmentRepository,
            BAVES.Api.Modules.Training.Repositories.AssignmentRepository>();

        services.AddScoped<BAVES.Api.Modules.Training.Services.Interfaces.IAssignmentService,
            BAVES.Api.Modules.Training.Services.AssignmentService>();

        services.AddHttpClient<BAVES.Api.Modules.Scenario.Services.Interfaces.IAiRuntimeClient,
            BAVES.Api.Modules.Scenario.Services.AiRuntimeClient>(client =>
        {
            var baseUrl = configuration["AiRuntime:BaseUrl"] ?? "http://localhost:8000";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(300);
        });

        // Module Commerce - Repositories
        services.AddScoped<BAVES.Api.Modules.Commerce.Repositories.Interfaces.IPlanRepository, BAVES.Api.Modules.Commerce.Repositories.PlanRepository>();
        services.AddScoped<BAVES.Api.Modules.Commerce.Repositories.Interfaces.IOrderRepository, BAVES.Api.Modules.Commerce.Repositories.OrderRepository>();
        services.AddScoped<BAVES.Api.Modules.Commerce.Repositories.Interfaces.ICartRepository, BAVES.Api.Modules.Commerce.Repositories.CartRepository>();
        services.AddScoped<BAVES.Api.Modules.Commerce.Repositories.Interfaces.IEntitlementRepository, BAVES.Api.Modules.Commerce.Repositories.EntitlementRepository>();
        services.AddScoped<BAVES.Api.Modules.Commerce.Repositories.Interfaces.ICreditLedgerRepository, BAVES.Api.Modules.Commerce.Repositories.CreditLedgerRepository>();
        services.AddScoped<BAVES.Api.Modules.Commerce.Repositories.Interfaces.IPaymentWebhookRepository, BAVES.Api.Modules.Commerce.Repositories.PaymentWebhookRepository>();

        // Module Commerce - Services & Gateway
        services.AddScoped<BAVES.Api.Modules.Commerce.Services.Interfaces.IPlanService, BAVES.Api.Modules.Commerce.Services.PlanService>();
        services.AddScoped<BAVES.Api.Modules.Commerce.Services.Interfaces.IOrderService, BAVES.Api.Modules.Commerce.Services.OrderService>();
        services.AddScoped<BAVES.Api.Modules.Commerce.Services.Interfaces.ICartService, BAVES.Api.Modules.Commerce.Services.CartService>();
        services.AddScoped<BAVES.Api.Modules.Commerce.Services.Interfaces.IPaymentWebhookService, BAVES.Api.Modules.Commerce.Services.PaymentWebhookService>();
        services.AddScoped<BAVES.Api.Modules.Commerce.Services.Interfaces.IEntitlementService, BAVES.Api.Modules.Commerce.Services.EntitlementService>();
        services.AddScoped<BAVES.Api.Modules.Commerce.Services.Interfaces.IScenarioPurchaseTargetLookup, BAVES.Api.Modules.Commerce.Services.ScenarioPurchaseTargetLookup>();
        services.AddHttpClient<BAVES.Api.Modules.Commerce.Gateways.PayOsGateway>();
        services.AddScoped<BAVES.Api.Modules.Commerce.Gateways.IPaymentGateway, BAVES.Api.Modules.Commerce.Gateways.PayOsGateway>();

        return services;
    }

    private static void AddModuleContext<TContext>(
        IServiceCollection services, string connectionString, string schema)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(3);
                sql.MigrationsHistoryTable("__EFMigrationsHistory", schema);
            }));
    }
}
