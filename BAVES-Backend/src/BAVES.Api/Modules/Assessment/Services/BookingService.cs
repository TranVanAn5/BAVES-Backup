using BAVES.Api.Modules.Assessment.Models.DTOs;
using BAVES.Api.Modules.Assessment.Models.Entities;
using BAVES.Api.Modules.Assessment.Models.Enums;
using BAVES.Api.Modules.Assessment.Repositories.Interfaces;
using BAVES.Api.Modules.Assessment.Services.Interfaces;
using BAVES.Api.Modules.Commerce.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BAVES.Api.Modules.Assessment.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IAssessmentRepository _assessmentRepo;
    private readonly IEntitlementService _entitlementService;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IBookingRepository bookingRepo,
        IAssessmentRepository assessmentRepo,
        IEntitlementService entitlementService,
        ILogger<BookingService> logger)
    {
        _bookingRepo = bookingRepo;
        _assessmentRepo = assessmentRepo;
        _entitlementService = entitlementService;
        _logger = logger;
    }

    public async Task<ExpertBookingDto> CreateBookingAsync(int learnerId, CreateExpertBookingRequest request, CancellationToken ct = default)
    {
        var reviewRequest = await _assessmentRepo.GetReviewRequestByIdAsync(request.ReviewRequestId, ct)
            ?? throw new KeyNotFoundException($"Không tìm thấy yêu cầu chấm bài ID: {request.ReviewRequestId}");

        if (reviewRequest.LearnerId != learnerId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền đặt lịch cho bài nộp này.");
        }

        if (reviewRequest.Status != ReviewRequestStatus.Completed)
        {
            throw new InvalidOperationException("Chỉ có thể đặt lịch trao đổi sau khi Chuyên gia đã hoàn tất chấm điểm bài nộp.");
        }

        if (!reviewRequest.AssignedReviewerId.HasValue)
        {
            throw new InvalidOperationException("Bài nộp chưa có Chuyên gia chấm điểm hợp lệ.");
        }

        var existingBooking = await _bookingRepo.GetByReviewRequestIdAsync(request.ReviewRequestId, ct);
        if (existingBooking != null && existingBooking.Status != BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Bài nộp này đã được đặt lịch trao đổi trước đó.");
        }

        var credits = await _entitlementService.GetConsultationCreditsAsync(learnerId, ct);
        if (credits < 1)
        {
            throw new InvalidOperationException("Bạn không đủ lượt booking trao đổi 1-on-1. Vui lòng đăng ký gói dịch vụ hoặc mua thêm lượt trao đổi.");
        }

        var booking = new ExpertBooking
        {
            ReviewRequestId = reviewRequest.Id,
            SubmissionId = reviewRequest.SubmissionId,
            LearnerId = learnerId,
            ExpertId = reviewRequest.AssignedReviewerId.Value,
            // Confirmed ngay khi đặt: phòng chat riêng với đúng expert đã chấm đã sẵn sàng.
            Status = BookingStatus.Confirmed,
            ScheduledStartTime = request.ScheduledStartTime,
            ScheduledEndTime = request.ScheduledEndTime,
            LearnerNote = request.LearnerNote,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _bookingRepo.AddBookingAsync(booking, ct);
        await _bookingRepo.SaveChangesAsync(ct);

        // Trừ 1 consultation credit
        await _entitlementService.ConsumeConsultationCreditAsync(learnerId, booking.Id, ct);

        _logger.LogInformation("Learner {LearnerId} booked 1-on-1 consultation {BookingId} with Expert {ExpertId}",
            learnerId, booking.Id, booking.ExpertId);

        return MapToDto(booking);
    }

    public async Task<List<ExpertBookingDto>> GetLearnerBookingsAsync(int learnerId, CancellationToken ct = default)
    {
        var list = await _bookingRepo.GetByLearnerIdAsync(learnerId, ct);
        return list.Select(MapToDto).ToList();
    }

    public async Task<List<ExpertBookingDto>> GetExpertBookingsAsync(int expertId, CancellationToken ct = default)
    {
        var list = await _bookingRepo.GetByExpertIdAsync(expertId, ct);
        return list.Select(MapToDto).ToList();
    }

    public async Task<BookingChatDossierDto> GetBookingChatDossierAsync(int currentUserId, int bookingId, bool isExpert, CancellationToken ct = default)
    {
        var booking = await _bookingRepo.GetByIdWithMessagesAsync(bookingId, ct)
            ?? throw new KeyNotFoundException($"Không tìm thấy buổi trao đổi ID: {bookingId}");

        if (!isExpert && booking.LearnerId != currentUserId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập buổi trao đổi này.");
        }

        if (isExpert && booking.ExpertId != currentUserId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập buổi trao đổi này.");
        }

        // Lấy kết quả chấm điểm của Expert cho bài nộp này
        var humanReview = await _assessmentRepo.GetHumanReviewDtoBySubmissionIdAsync(booking.SubmissionId, ct);

        var messageDtos = booking.Messages.Select(m => new ConsultationMessageDto(
            m.Id,
            m.BookingId,
            m.SenderId,
            m.SenderRole,
            m.Content,
            m.SentAt,
            m.IsRead
        )).ToList();

        return new BookingChatDossierDto(MapToDto(booking), humanReview, messageDtos);
    }

    public async Task<ConsultationMessageDto> SendMessageAsync(int currentUserId, int bookingId, string content, bool isExpert, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Nội dung tin nhắn không được để trống.", nameof(content));
        }

        var booking = await _bookingRepo.GetByIdAsync(bookingId, ct)
            ?? throw new KeyNotFoundException($"Không tìm thấy buổi trao đổi ID: {bookingId}");

        if (!isExpert && booking.LearnerId != currentUserId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền gửi tin nhắn trong phòng chat này.");
        }

        if (isExpert && booking.ExpertId != currentUserId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền gửi tin nhắn trong phòng chat này.");
        }

        if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Buổi trao đổi này đã kết thúc hoặc đã bị hủy, không thể gửi thêm tin nhắn.");
        }

        // Nếu đang ở Pending hoặc Confirmed, chuyển sang InProgress khi bắt đầu chat
        if (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed)
        {
            booking.Status = BookingStatus.InProgress;
            booking.ActualStartTime ??= DateTimeOffset.UtcNow;
            await _bookingRepo.UpdateBookingAsync(booking, ct);
        }

        var message = new ConsultationMessage
        {
            BookingId = bookingId,
            SenderId = currentUserId,
            SenderRole = isExpert ? "Expert" : "Learner",
            Content = content.Trim(),
            SentAt = DateTimeOffset.UtcNow,
            IsRead = false
        };

        await _bookingRepo.AddMessageAsync(message, ct);
        await _bookingRepo.SaveChangesAsync(ct);

        return new ConsultationMessageDto(
            message.Id,
            message.BookingId,
            message.SenderId,
            message.SenderRole,
            message.Content,
            message.SentAt,
            message.IsRead
        );
    }

    public async Task<ExpertBookingDto> UpdateBookingStatusAsync(int currentUserId, int bookingId, UpdateBookingStatusRequest request, bool isExpert, CancellationToken ct = default)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId, ct)
            ?? throw new KeyNotFoundException($"Không tìm thấy buổi trao đổi ID: {bookingId}");

        if (!isExpert && booking.LearnerId != currentUserId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền cập nhật buổi trao đổi này.");
        }

        if (isExpert && booking.ExpertId != currentUserId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền cập nhật buổi trao đổi này.");
        }

        booking.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.ExpertSummaryNote))
        {
            booking.ExpertSummaryNote = request.ExpertSummaryNote.Trim();
        }

        if (request.Status == BookingStatus.InProgress && !booking.ActualStartTime.HasValue)
        {
            booking.ActualStartTime = DateTimeOffset.UtcNow;
        }
        else if (request.Status == BookingStatus.Completed && !booking.ActualEndTime.HasValue)
        {
            booking.ActualEndTime = DateTimeOffset.UtcNow;
        }

        await _bookingRepo.UpdateBookingAsync(booking, ct);
        await _bookingRepo.SaveChangesAsync(ct);

        return MapToDto(booking);
    }

    public async Task<ExpertBookingDto> CancelBookingAsync(int currentUserId, int bookingId, string? cancelReason, bool isExpert, CancellationToken ct = default)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId, ct)
            ?? throw new KeyNotFoundException($"Không tìm thấy buổi trao đổi ID: {bookingId}");

        if (!isExpert && booking.LearnerId != currentUserId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền hủy buổi trao đổi này.");
        }

        if (isExpert && booking.ExpertId != currentUserId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền hủy buổi trao đổi này.");
        }

        if (booking.Status == BookingStatus.Completed)
        {
            throw new InvalidOperationException("Không thể hủy buổi trao đổi đã hoàn tất.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return MapToDto(booking);
        }

        booking.Status = BookingStatus.Cancelled;
        booking.ActualEndTime = DateTimeOffset.UtcNow;
        await _bookingRepo.UpdateBookingAsync(booking, ct);
        await _bookingRepo.SaveChangesAsync(ct);

        // Hoàn lại 1 consultation credit cho learner
        await _entitlementService.RefundConsultationCreditAsync(
            booking.LearnerId,
            booking.Id,
            cancelReason ?? "Hủy lịch hẹn trao đổi 1-on-1",
            ct);

        return MapToDto(booking);
    }

    private static ExpertBookingDto MapToDto(ExpertBooking b) =>
        new(
            b.Id,
            b.ReviewRequestId,
            b.SubmissionId,
            b.LearnerId,
            b.ExpertId,
            b.Status,
            b.ScheduledStartTime,
            b.ScheduledEndTime,
            b.ActualStartTime,
            b.ActualEndTime,
            b.LearnerNote,
            b.ExpertSummaryNote,
            b.CreatedAt
        );
}
