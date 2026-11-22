using ScenarioEntity = BAVES.Api.Modules.Scenario.Models.Entities.Scenario;
using ScenarioVersionEntity = BAVES.Api.Modules.Scenario.Models.Entities.ScenarioVersion;
using StakeholderPersonaEntity = BAVES.Api.Modules.Scenario.Models.Entities.StakeholderPersona;
using KnowledgeItemEntity = BAVES.Api.Modules.Scenario.Models.Entities.KnowledgeItem;
using StakeholderKnowledgeAccessEntity = BAVES.Api.Modules.Scenario.Models.Entities.StakeholderKnowledgeAccess;
using ScenarioBusinessRuleEntity = BAVES.Api.Modules.Scenario.Models.Entities.ScenarioBusinessRule;
using ScenarioEventEntity = BAVES.Api.Modules.Scenario.Models.Entities.ScenarioEvent;
using ScenarioSprintEntity = BAVES.Api.Modules.Scenario.Models.Entities.ScenarioSprint;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Scenario.Data;

public sealed class ScenarioDbContext(DbContextOptions<ScenarioDbContext> options) : DbContext(options)
{
    public DbSet<ScenarioEntity> Scenarios => Set<ScenarioEntity>();
    public DbSet<ScenarioVersionEntity> ScenarioVersions => Set<ScenarioVersionEntity>();
    public DbSet<StakeholderPersonaEntity> StakeholderPersonas => Set<StakeholderPersonaEntity>();
    public DbSet<KnowledgeItemEntity> KnowledgeItems => Set<KnowledgeItemEntity>();
    public DbSet<StakeholderKnowledgeAccessEntity> StakeholderKnowledgeAccess => Set<StakeholderKnowledgeAccessEntity>();
    public DbSet<ScenarioBusinessRuleEntity> BusinessRules => Set<ScenarioBusinessRuleEntity>();
    public DbSet<ScenarioEventEntity> ScenarioEvents => Set<ScenarioEventEntity>();
    public DbSet<ScenarioSprintEntity> ScenarioSprints => Set<ScenarioSprintEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("scenario");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScenarioDbContext).Assembly,
            type => type.Namespace == "BAVES.Api.Modules.Scenario.Data.Configurations");
    }
}
