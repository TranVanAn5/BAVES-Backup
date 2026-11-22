using BabokTaskEntity = BAVES.Api.Modules.Assessment.Models.Entities.BabokTask;
using RubricEntity = BAVES.Api.Modules.Assessment.Models.Entities.Rubric;
using RubricCriterionEntity = BAVES.Api.Modules.Assessment.Models.Entities.RubricCriterion;
using AIPracticeFeedbackEntity = BAVES.Api.Modules.Assessment.Models.Entities.AIPracticeFeedback;
using AIFeedbackCriterionScoreEntity = BAVES.Api.Modules.Assessment.Models.Entities.AIFeedbackCriterionScore;
using HumanReviewEntity = BAVES.Api.Modules.Assessment.Models.Entities.HumanReview;
using HumanReviewCriterionScoreEntity = BAVES.Api.Modules.Assessment.Models.Entities.HumanReviewCriterionScore;
using ReviewRequestEntity = BAVES.Api.Modules.Assessment.Models.Entities.ReviewRequest;
using ExpertBookingEntity = BAVES.Api.Modules.Assessment.Models.Entities.ExpertBooking;
using ConsultationMessageEntity = BAVES.Api.Modules.Assessment.Models.Entities.ConsultationMessage;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Assessment.Data;

public sealed class AssessmentDbContext(DbContextOptions<AssessmentDbContext> options) : DbContext(options)
{
    public DbSet<BabokTaskEntity> BabokTasks => Set<BabokTaskEntity>();
    public DbSet<RubricEntity> Rubrics => Set<RubricEntity>();
    public DbSet<RubricCriterionEntity> RubricCriteria => Set<RubricCriterionEntity>();
    public DbSet<AIPracticeFeedbackEntity> AIPracticeFeedbacks => Set<AIPracticeFeedbackEntity>();
    public DbSet<AIFeedbackCriterionScoreEntity> AIFeedbackCriterionScores => Set<AIFeedbackCriterionScoreEntity>();
    public DbSet<HumanReviewEntity> HumanReviews => Set<HumanReviewEntity>();
    public DbSet<HumanReviewCriterionScoreEntity> HumanReviewCriterionScores => Set<HumanReviewCriterionScoreEntity>();
    public DbSet<ReviewRequestEntity> ReviewRequests => Set<ReviewRequestEntity>();
    public DbSet<ExpertBookingEntity> ExpertBookings => Set<ExpertBookingEntity>();
    public DbSet<ConsultationMessageEntity> ConsultationMessages => Set<ConsultationMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("assessment");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssessmentDbContext).Assembly,
            type => type.Namespace == "BAVES.Api.Modules.Assessment.Data.Configurations");
    }
}
