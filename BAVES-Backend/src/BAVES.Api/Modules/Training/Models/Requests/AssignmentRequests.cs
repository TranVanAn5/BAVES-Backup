using System.ComponentModel.DataAnnotations;
using BAVES.Api.Modules.Training.Models.Enums;

namespace BAVES.Api.Modules.Training.Models.Requests;

/// <summary>
/// Payload tạo mới một Bài tập (Assignment).
/// Ràng buộc: Phải truyền đúng 1 trong 2 giá trị CohortId (giao theo lớp) hoặc LearnerId (giao cá nhân).
/// </summary>
public sealed class CreateAssignmentRequest
{
    [Required(ErrorMessage = "Tiêu đề bài tập (Title) là bắt buộc.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Tiêu đề bài tập phải từ 3 đến 200 ký tự.")]
    public string Title { get; set; } = null!;

    [StringLength(4000, ErrorMessage = "Hướng dẫn làm bài tối đa 4000 ký tự.")]
    public string? Instructions { get; set; }

    [Required(ErrorMessage = "ScenarioVersionId là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "ScenarioVersionId phải là số nguyên dương.")]
    public int ScenarioVersionId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "CohortId phải là số nguyên dương.")]
    public int? CohortId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "LearnerId phải là số nguyên dương.")]
    public int? LearnerId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "RubricId phải là số nguyên dương.")]
    public int? RubricId { get; set; }

    public DateTimeOffset? StartAt { get; set; }

    public DateTimeOffset? DueAt { get; set; }

    [Range(1, 100, ErrorMessage = "Số lần làm tối đa (MaxAttempts) phải từ 1 đến 100.")]
    public int? MaxAttempts { get; set; }

    public AssignmentStatus? Status { get; set; }
}

/// <summary>
/// Payload cập nhật thông tin Bài tập (Assignment).
/// </summary>
public sealed class UpdateAssignmentRequest
{
    [Required(ErrorMessage = "Tiêu đề bài tập (Title) là bắt buộc.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Tiêu đề bài tập phải từ 3 đến 200 ký tự.")]
    public string Title { get; set; } = null!;

    [StringLength(4000, ErrorMessage = "Hướng dẫn làm bài tối đa 4000 ký tự.")]
    public string? Instructions { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "RubricId phải là số nguyên dương.")]
    public int? RubricId { get; set; }

    public DateTimeOffset? StartAt { get; set; }

    public DateTimeOffset? DueAt { get; set; }

    [Range(1, 100, ErrorMessage = "Số lần làm tối đa (MaxAttempts) phải từ 1 đến 100.")]
    public int? MaxAttempts { get; set; }
}
