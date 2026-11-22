using BAVES.Api.Modules.Content.Models.DTOs;
using BAVES.Api.Modules.Content.Models.Enums;
using BAVES.Api.Modules.Content.Repositories.Interfaces;
using BAVES.Api.Modules.Content.Services.Interfaces;
using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Models.Enums;
using BAVES.Api.Modules.Scenario.Services;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using BAVES.Api.Modules.Training.Services.Interfaces;

namespace BAVES.Api.Modules.Content.Services;

public sealed class ContentReadService(
    IContentReadRepository repository,
    IScenarioCatalogService scenarioCatalog,
    IAssignmentService assignmentService) : IContentReadService
{
    public async Task<IReadOnlyList<IndustryDto>> GetIndustriesAsync(
        CancellationToken cancellationToken = default)
    {
        return await repository.GetActiveIndustriesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ApplicationDto>> GetApplicationsAsync(
        int? industryId,
        CancellationToken cancellationToken = default)
    {
        return await repository.GetActiveApplicationsAsync(industryId, cancellationToken);
    }

    public async Task<IReadOnlyList<ScenarioDto>> GetScenariosAsync(
        int userId,
        IReadOnlyCollection<string> userRoles,
        int? applicationId,
        bool? isPublished,
        CancellationToken cancellationToken = default)
    {
        var visibility = await BuildVisibilityFilterAsync(userId, userRoles, cancellationToken);
        var scenarios = await scenarioCatalog.GetScenariosAsync(
            applicationId,
            visibility,
            cancellationToken);

        if (scenarios.Count == 0)
        {
            return Array.Empty<ScenarioDto>();
        }

        var applicationIds = scenarios
            .Select(s => s.ApplicationId)
            .Distinct()
            .ToList();

        var applications = await repository.GetApplicationsByIdsAsync(applicationIds, cancellationToken);

        var result = scenarios
            .Select(s =>
            {
                applications.TryGetValue(s.ApplicationId, out var application);

                return new ScenarioDto
                {
                    Id = s.Id,
                    ApplicationId = s.ApplicationId,
                    Code = s.Code,
                    Title = s.Title,
                    Description = s.Description,
                    Status = s.Status switch
                    {
                        ScenarioStatus.Published => ContentStatus.Active,
                        ScenarioStatus.Draft => ContentStatus.Inactive,
                        _ => ContentStatus.Archived
                    },
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    HasPublishedVersion = s.HasPublishedVersion,
                    PublishedVersionId = s.PublishedVersionId,
                    Difficulty = s.Difficulty,
                    IsFreeAccess = s.IsFreeAccess,
                    Application = new ScenarioApplicationDto
                    {
                        Id = application?.Id ?? s.ApplicationId,
                        Code = application?.Code ?? string.Empty,
                        Name = application?.Name ?? string.Empty
                    }
                };
            })
            .ToList();

        if (isPublished == true)
        {
            result = result.Where(s => s.HasPublishedVersion).ToList();
        }

        return result;
    }

    public async Task<bool> ApplicationExistsAsync(
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        return await repository.ApplicationExistsAsync(applicationId, cancellationToken);
    }

    private async Task<ScenarioCatalogVisibilityFilter> BuildVisibilityFilterAsync(
        int userId,
        IReadOnlyCollection<string> userRoles,
        CancellationToken cancellationToken)
    {
        // Admin: thấy mọi kịch bản đã duyệt (kể cả đề nội bộ của instructor).
        if (ScenarioReviewRoles.IsAdmin(userRoles))
        {
            return new ScenarioCatalogVisibilityFilter { SeeAllPublished = true };
        }

        // Instructor: kịch bản mình tạo + Scenario Library (IsFreeAccess) để giao bài / tham khảo.
        if (ScenarioReviewRoles.IsInstructor(userRoles))
        {
            return new ScenarioCatalogVisibilityFilter
            {
                OwnerUserId = userId,
                IncludeFreeAccessPublished = true
            };
        }

        // Learner: bài được giao + kịch bản Admin đã duyệt (IsFreeAccess = Scenario Library toàn hệ thống).
        var assignedVersionIds = await assignmentService.GetVisibleScenarioVersionIdsForLearnerAsync(
            userId,
            cancellationToken);

        return new ScenarioCatalogVisibilityFilter
        {
            AllowedPublishedVersionIds = assignedVersionIds,
            IncludeFreeAccessPublished = true
        };
    }
}
