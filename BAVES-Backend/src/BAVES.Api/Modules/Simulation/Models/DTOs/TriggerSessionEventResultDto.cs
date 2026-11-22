using BAVES.Api.Modules.Simulation.Models.Enums;

namespace BAVES.Api.Modules.Simulation.Models.DTOs;

/// <summary>
/// Kết quả của việc trigger một event trong session: bản ghi SessionEvent mới
/// (hoặc bản ghi đã tồn tại nếu event đã từng được trigger trước đó).
/// </summary>
/// <param name="Event">Bản ghi event trong session.</param>
/// <param name="AlreadyTriggered">
/// true nếu event này đã được trigger trước đó và KHÔNG tạo bản ghi mới
/// (idempotent). false nếu đây là lần trigger đầu tiên.
/// </param>
public sealed record TriggerSessionEventResultDto(
    SessionEventDto Event,
    bool AlreadyTriggered);
