using BAPlanEntity = BAVES.Api.Modules.Planning.Models.Entities.BAPlan;
using PlannedQuestionEntity = BAVES.Api.Modules.Planning.Models.Entities.PlannedQuestion;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Planning.Data;

public sealed class PlanningDbContext(DbContextOptions<PlanningDbContext> options) : DbContext(options)
{
    public DbSet<BAPlanEntity> BAPlans => Set<BAPlanEntity>();
    public DbSet<PlannedQuestionEntity> PlannedQuestions => Set<PlannedQuestionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("planning");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlanningDbContext).Assembly,
            type => type.Namespace == "BAVES.Api.Modules.Planning.Data.Configurations");
    }
}
