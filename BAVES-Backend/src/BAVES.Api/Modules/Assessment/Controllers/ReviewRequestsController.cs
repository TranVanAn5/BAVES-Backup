using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Assessment.Models.DTOs;
using BAVES.Api.Modules.Assessment.Models.Requests;
using BAVES.Api.Modules.Assessment.Services.Interfaces;
using BAVES.Api.Modules.Identity.Services;
using BAVES.Api.Shared.Authorization;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Assessment.Controllers;

/// <summary>
/// Quản lý quy trình Đánh giá bởi Chuyên gia (Human Expert Review Workflow),
/// bao gồm tiếp nhận yêu cầu, xem hồ sơ chứng cứ (Case Dossier), chấm điểm Rubric,
/// và xuất Báo cáo đánh giá tổng hợp.
/// </summary>
[ApiController]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ReviewRequestsController(IAssessmentService assessmentService) : ControllerBase
{
    private int GetUserIdOrThrow()
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub),
            NumberStyles.None, CultureInfo.InvariantCulture, out var userId))
        {
            throw new UnauthorizedAccessException();
        }
        return userId;
    }

    private string GetUserRole()
    {
        if (User.IsInRole("ADMIN")) return "ADMIN";
        if (User.IsInRole("INSTRUCTOR")) return "INSTRUCTOR";
        if (User.IsInRole("EXPERT") || User.IsInRole("BAExpert") || User.IsInRole("BAEXPERT")) return "EXPERT";
        return "LEARNER";
    }

    /// <summary>
    /// Học viên gửi yêu cầu Chuyên gia đánh giá bài nộp (Submission).
    /// </summary>
    [HttpPost("/api/submissions/{submissionId:int}/review-requests")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.LearnerOnly)]
    [ProducesResponseType<ReviewRequestDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReviewRequestDto>> CreateReviewRequest(
        [FromRoute] int submissionId,
        [FromBody] CreateReviewRequestDto request,
        CancellationToken cancellationToken)
    {
        var learnerId = GetUserIdOrThrow();

        try
        {
            var result = await assessmentService.CreateReviewRequestAsync(
                learnerId,
                submissionId,
                request,
                cancellationToken);

            if (result is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Submission Not Found",
                    Detail = $"Không tìm thấy bài nộp ID {submissionId}."
                });
            }

            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Review Request Conflict",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Lấy danh sách các yêu cầu đánh giá.
    /// Learner: xem các yêu cầu của mình.
    /// Expert/Instructor: xem hàng đợi chung (scope=queue) hoặc các yêu cầu đã tiếp nhận (scope=mine).
    /// </summary>
    [HttpGet("/api/review-requests")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.AnyAuthenticated)]
    [ProducesResponseType<IReadOnlyList<ReviewRequestDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ReviewRequestDto>>> GetReviewRequests(
        [FromQuery] string? scope,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();

        var requests = await assessmentService.GetReviewRequestsAsync(
            userId,
            role,
            scope,
            cancellationToken);

        return this.OkPaged(requests, page, pageSize);
    }

    /// <summary>
    /// Chuyên gia tiếp nhận một yêu cầu đánh giá từ hàng đợi (Atomic Concurrency Lock).
    /// Ngăn chặn trường hợp 2 chuyên gia cùng tiếp nhận một bài nộp đồng thời.
    /// </summary>
    [HttpPost("/api/review-requests/{id:int}/accept")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.ExpertOrInstructor)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AcceptReviewRequest(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var reviewerId = GetUserIdOrThrow();

        var success = await assessmentService.AcceptReviewRequestAsync(
            reviewerId,
            id,
            cancellationToken);

        if (!success)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Accept Failed",
                Detail = "Yêu cầu đánh giá không tồn tại hoặc đã được chuyên gia khác tiếp nhận."
            });
        }

        return Ok(new { message = "Tiếp nhận yêu cầu đánh giá thành công." });
    }

    /// <summary>
    /// Chuyên gia lấy Hồ sơ kiểm thử nghiệp vụ (Case Dossier) để chấm bài.
    /// Hồ sơ bao gồm: Tài liệu BA Artifact, Kế hoạch BA Plan, Danh sách Phân tích và Bằng chứng phỏng vấn, Đánh giá AI sơ bộ, và Rubric chấm điểm.
    /// </summary>
    [HttpGet("/api/review-requests/{id:int}/case")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.ExpertOrInstructor)]
    [ProducesResponseType<ReviewCaseDossierDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewCaseDossierDto>> GetReviewCaseDossier(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var reviewerId = GetUserIdOrThrow();

        try
        {
            var dossier = await assessmentService.GetReviewCaseDossierAsync(
                reviewerId,
                id,
                cancellationToken);

            if (dossier is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = $"Không tìm thấy yêu cầu đánh giá ID {id}."
                });
            }

            return Ok(dossier);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource Not Found",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Chuyên gia hoàn thành chấm điểm theo Rubric, nhập nhận xét và chốt điểm chính thức (FinalScore).
    /// Hệ thống sẽ tự động tính điểm theo trọng số Rubric và chuyển trạng thái Submission thành Finalized.
    /// </summary>
    [HttpPost("/api/review-requests/{id:int}/complete")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.ExpertOrInstructor)]
    [ProducesResponseType<HumanReviewDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HumanReviewDto>> CompleteReview(
        [FromRoute] int id,
        [FromBody] CompleteReviewRequest request,
        CancellationToken cancellationToken)
    {
        var reviewerId = GetUserIdOrThrow();

        try
        {
            var review = await assessmentService.CompleteReviewAsync(
                reviewerId,
                id,
                request,
                cancellationToken);

            if (review is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = $"Không tìm thấy yêu cầu đánh giá ID {id}."
                });
            }

            return Ok(review);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource Not Found",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Chuyên gia / giảng viên yêu cầu học viên làm lại bài (RevisionRequested).
    /// Lý do được gắn vào RequestNote; học viên có thể nộp lại và tạo yêu cầu chấm mới.
    /// </summary>
    [HttpPost("/api/review-requests/{id:int}/request-revision")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.ExpertOrInstructor)]
    [ProducesResponseType<ReviewRequestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReviewRequestDto>> RequestRevision(
        [FromRoute] int id,
        [FromBody] RequestRevisionRequest request,
        CancellationToken cancellationToken)
    {
        var reviewerId = GetUserIdOrThrow();

        try
        {
            var result = await assessmentService.RequestRevisionAsync(
                reviewerId,
                id,
                request,
                cancellationToken);

            if (result is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = $"Không tìm thấy yêu cầu đánh giá ID {id}."
                });
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Xuất Báo cáo đánh giá tổng hợp (Final Submission Report).
    /// Bao gồm phản hồi AI sơ bộ, đánh giá của chuyên gia, điểm chính thức và trạng thái chốt bài.
    /// </summary>
    [HttpGet("/api/submissions/{submissionId:int}/report")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.AnyAuthenticated)]
    [ProducesResponseType<FinalSubmissionReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FinalSubmissionReportDto>> GetFinalReport(
        [FromRoute] int submissionId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();

        try
        {
            var report = await assessmentService.GetFinalReportAsync(
                userId,
                role,
                submissionId,
                cancellationToken);

            if (report is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Report Not Found",
                    Detail = $"Không tìm thấy báo cáo cho bài nộp ID {submissionId}."
                });
            }

            return Ok(report);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = ex.Message
            });
        }
    }
}
