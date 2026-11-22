using BAVES.Api.Modules.Scenario.Models.Enums;

namespace BAVES.Api.Modules.Simulation.Models.DTOs;

/// <summary>
/// Thông tin một event được định nghĩa trong ScenarioVersion mà learner có thể trigger.
/// Dùng làm "danh mục" cho client hiển thị nút kích hoạt; KHÔNG phải bản ghi event đã xảy ra.
/// </summary>
/// <param name="Id">Id của ScenarioEvent.</param>
/// <param name="Title">Tiêu đề event hiển thị cho learner.</param>
/// <param name="Description">Mô tả ngắn gọn về event.</param>
/// <param name="TriggerType">Kiểu kích hoạt: Manual / AfterSprintEnds / …</param>
/// <param name="DisplayOrder">Thứ tự hiển thị trong kịch bản.</param>
/// <param name="IsTriggered">Event đã được trigger trong session này hay chưa.</param>
/// <param name="TriggeredAt">Thời điểm đã trigger (null nếu chưa trigger).</param>
/// <param name="AfterSprintNo">Sprint N phải kết thúc trước khi event xuất hiện (AfterSprintEnds).</param>
/// <param name="IsUnlocked">Đã đủ điều kiện thời điểm (Manual = luôn true cho Instructor; AfterSprintEnds = sprint N đã finalize).</param>
/// <param name="UnlockHint">Gợi ý thời điểm xuất hiện (VD: "Sau khi kết thúc Sprint 2").</param>
public sealed record ScenarioEventAvailabilityDto(
    int Id,
    string Title,
    string Description,
    ScenarioEventTriggerType TriggerType,
    int DisplayOrder,
    bool IsTriggered,
    DateTimeOffset? TriggeredAt,
    int? AfterSprintNo = null,
    bool IsUnlocked = true,
    string? UnlockHint = null);
