using System.ComponentModel.DataAnnotations;

namespace BAVES.Api.Modules.Training.Models.Requests;

/// <summary>
/// Payload thêm 1 học viên vào Lớp học (truyền LearnerId hoặc Email).
/// </summary>
public sealed class AddCohortMemberRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "LearnerId phải là số nguyên dương.")]
    public int? LearnerId { get; set; }

    [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
    [StringLength(256, ErrorMessage = "Email tối đa 256 ký tự.")]
    public string? Email { get; set; }
}

/// <summary>
/// Payload thêm danh sách học viên hàng loạt vào Lớp học (qua danh sách LearnerIds hoặc danh sách Emails).
/// </summary>
public sealed class BatchAddCohortMembersRequest
{
    public List<int>? LearnerIds { get; set; }

    public List<string>? Emails { get; set; }
}
