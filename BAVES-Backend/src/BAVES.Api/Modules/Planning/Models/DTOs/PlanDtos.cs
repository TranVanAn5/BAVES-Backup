using BAVES.Api.Modules.Planning.Models.Enums;

namespace BAVES.Api.Modules.Planning.Models.DTOs;

/// <summary>
/// Dữ liệu trả về cho client khi gọi GET/api/sessions/{id}plan.
/// Chứa thông tin kế hoạch BA (mục tiêu, cách tiếp cận, trạng thái)
/// và danh sách các câu hỏi mà learner dự định sẽ hỏi stakeholders.
/// </summary>
// record tự sinh Equals/GetHashCode/toString.
// record mặc định chỉ đọc
// sealed ko cho class khác kế thừa
public sealed record BAPlanDto(
    int Id,
    int SessionId,
    string Objective,
    string? Approach,
    BAPlanStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<PlannedQuestionDto> Questions
);
///<summary>
///DTO cho PlannedQuestion - một câu hỏi Learner dự định hỏi trong phiên.
/// </summary>
public sealed record PlannedQuestionDto(
    int Id,
    int? StakeholderPersonaId,
    string? Topic,
    string? InformationNeed,
    string Question,
    PriorityLevel Priority,
    int DisplayOrder,
    PlannedQuestionStatus Status,
    DateTimeOffset CreatedAt
);

/// <summary>
/// Payload client gửi lên khi PUT /api/sessions/{id}/plan.
/// Objective: bắt buộc (vì đây là "mục tiêu BA")
/// Questions: tùy chọn (3 trạng thái: null = không đụng, [] = xóa hết, có phần tử = thay mới)
/// </summary>

public sealed record UpsertPlanRequest(
    string Objective,
    string? Approach,
    IReadOnlyList<PlannedQuestionInput>? Questions
    );

/// <summary>
/// 1 câu hỏi learner gửi lên. Không có Id vì server tự sinh.
/// PlannedQuestionInput chỉ là "dữ liệu đầu vào", nó KHÁC với PlannedQuestionDto (dữ liệu trả về).
/// </summary>
public sealed record PlannedQuestionInput(
    int? StakeholderPersonaId,
    string? Topic,
    string? InformationNeed,
    string Question,
    PriorityLevel Priority,
    int DisplayOrder
    );


