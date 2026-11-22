using System.ComponentModel.DataAnnotations;
using BAVES.Api.Modules.Training.Models.Enums;

namespace BAVES.Api.Modules.Training.Models.Requests;

/// <summary>
/// Payload tạo mới một Lớp học (Cohort).
/// </summary>
public sealed class CreateCohortRequest
{
    [Required(ErrorMessage = "Mã lớp học (Code) là bắt buộc.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Mã lớp học phải từ 2 đến 80 ký tự.")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Tên lớp học (Name) là bắt buộc.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Tên lớp học phải từ 3 đến 200 ký tự.")]
    public string Name { get; set; } = null!;

    [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public CohortStatus? Status { get; set; }
}

/// <summary>
/// Payload cập nhật thông tin Lớp học (Cohort).
/// </summary>
public sealed class UpdateCohortRequest
{
    [Required(ErrorMessage = "Mã lớp học (Code) là bắt buộc.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Mã lớp học phải từ 2 đến 80 ký tự.")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Tên lớp học (Name) là bắt buộc.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Tên lớp học phải từ 3 đến 200 ký tự.")]
    public string Name { get; set; } = null!;

    [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}

/// <summary>
/// Payload chuyển trạng thái của Lớp học (Draft, Active, Completed, Archived).
/// </summary>
public sealed class UpdateCohortStatusRequest
{
    [Required(ErrorMessage = "Trạng thái lớp học (Status) là bắt buộc.")]
    public CohortStatus Status { get; set; }
}

/// <summary>
/// Payload học viên gửi yêu cầu tham gia lớp học bằng Mã lớp (Code).
/// </summary>
public sealed class JoinCohortByCodeRequest
{
    [Required(ErrorMessage = "Mã lớp học (Code) là bắt buộc.")]
    [StringLength(80, MinimumLength = 2)]
    public string Code { get; set; } = null!;
}
