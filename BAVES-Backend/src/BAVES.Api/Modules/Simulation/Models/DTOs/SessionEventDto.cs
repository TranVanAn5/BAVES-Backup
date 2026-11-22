using BAVES.Api.Modules.Simulation.Models.Enums;

namespace BAVES.Api.Modules.Simulation.Models.DTOs;

/// <summary>
/// Một sự kiện (event) đã xảy ra trong một Simulation Session.
/// Đây là "bản ghi" event tại thời điểm kích hoạt: <see cref="EventContentSnapshot"/>
/// giữ nội dung của ScenarioEvent tại lúc trigger, nên nếu kịch bản được sửa sau đó
/// thì dữ liệu learner đã thấy vẫn không bị thay đổi.
/// </summary>
/// <param name="Id">Id bản ghi SessionEvent.</param>
/// <param name="SessionId">Session chứa event.</param>
/// <param name="ScenarioEventId">Event gốc trong ScenarioVersion (ScenarioEvent.Id).</param>
/// <param name="TriggerSource">Nguồn kích hoạt: Automatic / Manual / Instructor.</param>
/// <param name="TriggeredAt">Thời điểm event được kích hoạt (UTC).</param>
/// <param name="SequenceNo">Số thứ tự event trong session (bắt đầu từ 1).</param>
/// <param name="EventContentSnapshot">Nội dung event đã đóng băng tại thời điểm trigger.</param>
public sealed record SessionEventDto(
    int Id,
    int SessionId,
    int ScenarioEventId,
    EventTriggerSource TriggerSource,
    DateTimeOffset TriggeredAt,
    int SequenceNo,
    string EventContentSnapshot
);

/// <summary>
/// Một trang danh sách event của session, kèm thông tin phân trang.
/// </summary>
public sealed record SessionEventPageDto(
    IReadOnlyList<SessionEventDto> Items,
    int Page,
    int PageSize,
    int Total
);
