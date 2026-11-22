using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Assessment.Models.DTOs;
using BAVES.Api.Modules.Assessment.Services.Interfaces;
using BAVES.Api.Shared.Authorization;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Assessment.Controllers;

[ApiController]
[Route("api/assessment/bookings")]
[Authorize]
[Tags("Assessment - Expert Bookings & 1-on-1 Chat")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// [Learner] Đặt lịch trao đổi 1-on-1 với Chuyên gia sau khi bài nộp đã được chấm điểm.
    /// Tự động trừ 1 Consultation Credit của Learner.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ExpertBookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateExpertBookingRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _bookingService.CreateBookingAsync(userId.Value, request, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// [Learner] Xem danh sách các lịch hẹn trao đổi 1-on-1 của mình.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(List<ExpertBookingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyBookings(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var list = await _bookingService.GetLearnerBookingsAsync(userId.Value, ct);
        return this.OkPagedAction(list, page, pageSize);
    }

    /// <summary>
    /// [Expert] Xem danh sách các lịch hẹn trao đổi 1-on-1 được gán cho mình.
    /// </summary>
    [HttpGet("expert")]
    [Authorize(Policy = Policies.ExpertOrInstructor)]
    [ProducesResponseType(typeof(List<ExpertBookingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpertBookings(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var list = await _bookingService.GetExpertBookingsAsync(userId.Value, ct);
        return this.OkPagedAction(list, page, pageSize);
    }

    /// <summary>
    /// Mở phòng trao đổi 1-on-1: Trả về thông tin booking, điểm số rubric và nhận xét của Chuyên gia, cùng lịch sử tin nhắn.
    /// </summary>
    [HttpGet("{id:int}/chat")]
    [ProducesResponseType(typeof(BookingChatDossierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookingChat(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var dossier = await _bookingService.GetBookingChatDossierAsync(userId.Value, id, IsExpertActor(), ct);
            return Ok(dossier);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gửi tin nhắn mới vào phòng trao đổi 1-on-1 giữa Learner và Expert.
    /// </summary>
    [HttpPost("{id:int}/messages")]
    [ProducesResponseType(typeof(ConsultationMessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendConsultationMessageRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var msg = await _bookingService.SendMessageAsync(userId.Value, id, request.Content, IsExpertActor(), ct);
            return Ok(msg);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cập nhật trạng thái buổi trao đổi (Chấp nhận, Bắt đầu, Hoàn tất buổi hẹn kèm ghi chú tổng kết).
    /// </summary>
    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(ExpertBookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _bookingService.UpdateBookingStatusAsync(userId.Value, id, request, IsExpertActor(), ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Hủy lịch hẹn trao đổi 1-on-1 và tự động hoàn trả 1 Consultation Credit cho Learner.
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(ExpertBookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelBooking(int id, [FromQuery] string? reason, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _bookingService.CancelBookingAsync(userId.Value, id, reason, IsExpertActor(), ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private int? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(sub, out var id) ? id : null;
    }

    private bool IsExpertActor() =>
        User.IsInRole("EXPERT") ||
        User.IsInRole("BAExpert") ||
        User.IsInRole("BAEXPERT") ||
        User.IsInRole("INSTRUCTOR") ||
        User.IsInRole("ADMIN") ||
        User.IsInRole("Instructor") ||
        User.IsInRole("Admin");
}
