using BAVES.Api.Modules.Scenario.Models.Enums;

namespace BAVES.Api.Modules.Scenario.Models.DTOs;

/// <summary>
/// Thông tin một event được định nghĩa trong ScenarioVersion, cung cấp cho module
/// Simulation khi cần validate và ghi nhận event đã kích hoạt trong session.
/// Không chứa dữ liệu riêng của learner.
/// </summary>
/// <param name="Id">Id của ScenarioEvent.</param>
/// <param name="ScenarioVersionId">Version chứa event.</param>
/// <param name="Title">Tiêu đề event.</param>
/// <param name="Description">Mô tả event.</param>
/// <param name="TriggerType">Cách kích hoạt: Manual / MessageCount / TimeElapsed / Condition.</param>
/// <param name="EventContent">Nội dung event (sẽ được đóng băng thành snapshot khi trigger).</param>
/// <param name="DisplayOrder">Thứ tự hiển thị.</param>
/// <param name="IsActive">Event còn hiệu lực hay đã bị tắt.</param>
public sealed record ScenarioEventInfoDto(
    int Id,
    int ScenarioVersionId,
    string Title,
    string Description,
    ScenarioEventTriggerType TriggerType,
    string EventContent,
    int DisplayOrder,
    bool IsActive,
    string? TriggerConditionJson = null
);
