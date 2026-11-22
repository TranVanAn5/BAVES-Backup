using BAVES.Api.Modules.Content.Models.DTOs;

namespace BAVES.Api.Modules.Content.Services.Interfaces;

public interface IContentReadService
{
    Task<IReadOnlyList<IndustryDto>> GetIndustriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApplicationDto>> GetApplicationsAsync(
        int? industryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScenarioDto>> GetScenariosAsync(
        int userId,
        IReadOnlyCollection<string> userRoles,
        int? applicationId,
        bool? isPublished,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra application có tồn tại hay không.
    /// Dùng bởi module khác để validate tham chiếu ApplicationId.
    /// </summary>
    Task<bool> ApplicationExistsAsync(
        int applicationId,
        CancellationToken cancellationToken = default);
}
