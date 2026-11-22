namespace BAVES.Api.Modules.Scenario.Models.Entities;

/// <summary>
/// Một sprint thuộc ScenarioVersion (lộ trình nhiệm vụ BA trong kịch bản).
/// Tách khỏi <see cref="ScenarioVersion.Assignment"/> (mô tả nhiệm vụ tổng quát)
/// và khỏi Training.Assignment (Instructor giao bài cho learner).
/// </summary>
public class ScenarioSprint
{
    public int Id { get; set; }

    public int ScenarioVersionId { get; set; }

    public int SprintNo { get; set; }

    /// <summary>Nội dung / nhiệm vụ của sprint.</summary>
    public string Focus { get; set; } = string.Empty;

    /// <summary>Số đơn vị thời hạn (tối thiểu 1).</summary>
    public int DurationValue { get; set; } = 1;

    /// <summary>"days" hoặc "weeks" — Sprint 1 từ lúc bắt đầu mô phỏng; sprint sau từ khi sprint trước kết thúc.</summary>
    public string DurationUnit { get; set; } = "weeks";

    /// <summary>Mã mẫu deliverable gắn sprint, lưu CSV (DEL-01,DEL-02).</summary>
    public string? TemplateCodes { get; set; }

    public ScenarioVersion ScenarioVersion { get; set; } = null!;
}
