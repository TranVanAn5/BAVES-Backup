using BAVES.Api.Modules.Content.Models.DTOs;

namespace BAVES.Api.Modules.Content.Repositories.Interfaces;

/// <summary>
/// Truy vấn dữ liệu Content (Industries, Applications, Scenarios) từ database.
/// </summary>
public interface IContentReadRepository
{
    /// <summary>
    /// Lấy danh sách industry đang Active, sắp theo DisplayOrder rồi Name.
    /// </summary>
    Task<IReadOnlyList<IndustryDto>> GetActiveIndustriesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách application đang Active. Nếu truyền industryId thì chỉ lấy
    /// các application thuộc industry đó.
    /// </summary>
    Task<IReadOnlyList<ApplicationDto>> GetActiveApplicationsAsync(
        int? industryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy các application theo danh sách Id, trả về dictionary Id → ApplicationDto.
    /// Dùng để tránh N+1 query khi gắn thông tin application vào nhiều scenario.
    /// </summary>
    Task<IReadOnlyDictionary<int, ApplicationDto>> GetApplicationsByIdsAsync(
        IReadOnlyList<int> applicationIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra một application có tồn tại hay không.
    /// Dùng để validate ApplicationId trước khi tạo dữ liệu tham chiếu tới nó.
    /// </summary>
    Task<bool> ApplicationExistsAsync(
        int applicationId,
        CancellationToken cancellationToken = default);
}
