using System.ComponentModel.DataAnnotations;

namespace BAVES.Api.Modules.Assessment.Models.Requests;

public sealed class CreateReviewRequestDto
{
    [StringLength(1000, ErrorMessage = "RequestNote tối đa 1000 ký tự.")]
    public string? RequestNote { get; set; }
}

public sealed class CriterionScoreInputRequest
{
    [Required(ErrorMessage = "RubricCriterionId là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "RubricCriterionId không hợp lệ.")]
    public int RubricCriterionId { get; set; }

    [Required(ErrorMessage = "Score là bắt buộc.")]
    [Range(0, 100, ErrorMessage = "Điểm phải nằm trong khoảng 0 đến 100.")]
    public decimal Score { get; set; }

    [StringLength(1000, ErrorMessage = "Comment tối đa 1000 ký tự.")]
    public string? Comment { get; set; }
}

public sealed class CompleteReviewRequest
{
    [Required(ErrorMessage = "OverallFeedback là bắt buộc.")]
    [StringLength(3000, MinimumLength = 10, ErrorMessage = "Nhận xét tổng thể phải từ 10 đến 3000 ký tự.")]
    public string OverallFeedback { get; set; } = null!;

    [Required(ErrorMessage = "Danh sách điểm tiêu chí không được rỗng.")]
    [MinLength(1, ErrorMessage = "Phải chấm ít nhất 1 tiêu chí.")]
    public List<CriterionScoreInputRequest> CriterionScores { get; set; } = new();
}

public sealed class RequestRevisionRequest
{
    [Required(ErrorMessage = "Reason là bắt buộc.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Lý do yêu cầu làm lại phải từ 10 đến 1000 ký tự.")]
    public string Reason { get; set; } = null!;
}
