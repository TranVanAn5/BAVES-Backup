using SimulationSessionEntity = BAVES.Api.Modules.Simulation.Models.Entities.SimulationSession;
using SessionAgentEntity = BAVES.Api.Modules.Simulation.Models.Entities.SessionAgent;
using MessageEntity = BAVES.Api.Modules.Simulation.Models.Entities.Message;
using SessionEventEntity = BAVES.Api.Modules.Simulation.Models.Entities.SessionEvent;
using SessionAgentKnowledgeStateEntity = BAVES.Api.Modules.Simulation.Models.Entities.SessionAgentKnowledgeState;
using CoachFeedbackEntity = BAVES.Api.Modules.Simulation.Models.Entities.CoachFeedback;
using SessionSprintReviewEntity = BAVES.Api.Modules.Simulation.Models.Entities.SessionSprintReview;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Simulation.Data;

public sealed class SimulationDbContext(DbContextOptions<SimulationDbContext> options) : DbContext(options)
{
    public DbSet<SimulationSessionEntity> SimulationSessions => Set<SimulationSessionEntity>();
    public DbSet<SessionAgentEntity> SessionAgents => Set<SessionAgentEntity>();
    public DbSet<MessageEntity> Messages => Set<MessageEntity>();
    public DbSet<SessionEventEntity> SessionEvents => Set<SessionEventEntity>();
    public DbSet<SessionAgentKnowledgeStateEntity> SessionAgentKnowledgeStates => Set<SessionAgentKnowledgeStateEntity>();
    public DbSet<CoachFeedbackEntity> CoachFeedbacks => Set<CoachFeedbackEntity>();
    public DbSet<SessionSprintReviewEntity> SessionSprintReviews => Set<SessionSprintReviewEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("simulation");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SimulationDbContext).Assembly,
            type => type.Namespace == "BAVES.Api.Modules.Simulation.Data.Configurations");
    }
}
