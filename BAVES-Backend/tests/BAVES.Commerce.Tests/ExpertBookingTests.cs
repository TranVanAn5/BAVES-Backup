using BAVES.Api.Modules.Assessment.Data;
using BAVES.Api.Modules.Assessment.Models.DTOs;
using BAVES.Api.Modules.Assessment.Models.Entities;
using BAVES.Api.Modules.Assessment.Models.Enums;
using BAVES.Api.Modules.Assessment.Repositories;
using BAVES.Api.Modules.Assessment.Services;
using BAVES.Api.Modules.Commerce.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BAVES.Commerce.Tests;

public class ExpertBookingTests
{
    private AssessmentDbContext CreateInMemoryAssessmentDb()
    {
        var options = new DbContextOptionsBuilder<AssessmentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AssessmentDbContext(options);
    }

    [Fact]
    public async Task CreateBookingAsync_ValidRequestWithCredits_CreatesConfirmedBookingAndConsumesCredit()
    {
        // Arrange
        using var db = CreateInMemoryAssessmentDb();
        var bookingRepo = new BookingRepository(db);
        var mockAssessmentRepo = new Mock<BAVES.Api.Modules.Assessment.Repositories.Interfaces.IAssessmentRepository>();
        var mockEntitlementService = new Mock<IEntitlementService>();

        const int learnerId = 10;
        const int expertId = 99;
        const int reviewRequestId = 1;
        const int submissionId = 5;

        var reviewRequest = new ReviewRequest
        {
            Id = reviewRequestId,
            SubmissionId = submissionId,
            LearnerId = learnerId,
            AssignedReviewerId = expertId,
            Status = ReviewRequestStatus.Completed,
            RequestedAt = DateTimeOffset.UtcNow.AddDays(-1),
            CompletedAt = DateTimeOffset.UtcNow.AddHours(-2)
        };

        mockAssessmentRepo.Setup(r => r.GetReviewRequestByIdAsync(reviewRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviewRequest);

        mockEntitlementService.Setup(s => s.GetConsultationCreditsAsync(learnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1); // 1 credit available

        var service = new BookingService(bookingRepo, mockAssessmentRepo.Object, mockEntitlementService.Object, NullLogger<BookingService>.Instance);

        var request = new CreateExpertBookingRequest(
            ReviewRequestId: reviewRequestId,
            ScheduledStartTime: DateTimeOffset.UtcNow.AddDays(1),
            ScheduledEndTime: DateTimeOffset.UtcNow.AddDays(1).AddMinutes(45),
            LearnerNote: "Em muốn hỏi thêm về phần Business Rules trong kịch bản OmniMart."
        );

        // Act
        var result = await service.CreateBookingAsync(learnerId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(BookingStatus.Confirmed, result.Status);
        Assert.Equal(expertId, result.ExpertId);
        Assert.Equal(learnerId, result.LearnerId);
        Assert.Equal(request.LearnerNote, result.LearnerNote);

        // Verify credit consumption called
        mockEntitlementService.Verify(s => s.ConsumeConsultationCreditAsync(learnerId, result.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBookingAsync_ReviewNotCompleted_ThrowsInvalidOperationException()
    {
        // Arrange
        using var db = CreateInMemoryAssessmentDb();
        var bookingRepo = new BookingRepository(db);
        var mockAssessmentRepo = new Mock<BAVES.Api.Modules.Assessment.Repositories.Interfaces.IAssessmentRepository>();
        var mockEntitlementService = new Mock<IEntitlementService>();

        const int learnerId = 10;
        const int reviewRequestId = 2;

        var pendingReviewRequest = new ReviewRequest
        {
            Id = reviewRequestId,
            SubmissionId = 6,
            LearnerId = learnerId,
            AssignedReviewerId = 99,
            Status = ReviewRequestStatus.Pending // Not completed yet
        };

        mockAssessmentRepo.Setup(r => r.GetReviewRequestByIdAsync(reviewRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingReviewRequest);

        var service = new BookingService(bookingRepo, mockAssessmentRepo.Object, mockEntitlementService.Object, NullLogger<BookingService>.Instance);

        var request = new CreateExpertBookingRequest(reviewRequestId, null, null, "Test");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateBookingAsync(learnerId, request));
        Assert.Contains("hoàn tất chấm điểm", ex.Message);
    }

    [Fact]
    public async Task CreateBookingAsync_InsufficientCredits_ThrowsInvalidOperationException()
    {
        // Arrange
        using var db = CreateInMemoryAssessmentDb();
        var bookingRepo = new BookingRepository(db);
        var mockAssessmentRepo = new Mock<BAVES.Api.Modules.Assessment.Repositories.Interfaces.IAssessmentRepository>();
        var mockEntitlementService = new Mock<IEntitlementService>();

        const int learnerId = 10;
        const int reviewRequestId = 3;

        var reviewRequest = new ReviewRequest
        {
            Id = reviewRequestId,
            SubmissionId = 7,
            LearnerId = learnerId,
            AssignedReviewerId = 99,
            Status = ReviewRequestStatus.Completed
        };

        mockAssessmentRepo.Setup(r => r.GetReviewRequestByIdAsync(reviewRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviewRequest);

        mockEntitlementService.Setup(s => s.GetConsultationCreditsAsync(learnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0); // 0 credits

        var service = new BookingService(bookingRepo, mockAssessmentRepo.Object, mockEntitlementService.Object, NullLogger<BookingService>.Instance);

        var request = new CreateExpertBookingRequest(reviewRequestId, null, null, "Test");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateBookingAsync(learnerId, request));
        Assert.Contains("không đủ lượt booking", ex.Message);
    }

    [Fact]
    public async Task SendMessageAsync_BetweenLearnerAndExpert_SucceedsAndUpdatesStatusToInProgress()
    {
        // Arrange
        using var db = CreateInMemoryAssessmentDb();
        var bookingRepo = new BookingRepository(db);
        var mockAssessmentRepo = new Mock<BAVES.Api.Modules.Assessment.Repositories.Interfaces.IAssessmentRepository>();
        var mockEntitlementService = new Mock<IEntitlementService>();

        const int learnerId = 10;
        const int expertId = 99;

        var reviewReq = new ReviewRequest
        {
            Id = 1,
            SubmissionId = 5,
            LearnerId = learnerId,
            AssignedReviewerId = expertId,
            Status = ReviewRequestStatus.Completed
        };
        db.ReviewRequests.Add(reviewReq);

        var booking = new ExpertBooking
        {
            ReviewRequestId = reviewReq.Id,
            SubmissionId = 5,
            LearnerId = learnerId,
            ExpertId = expertId,
            Status = BookingStatus.Confirmed
        };
        db.ExpertBookings.Add(booking);
        await db.SaveChangesAsync();

        var service = new BookingService(bookingRepo, mockAssessmentRepo.Object, mockEntitlementService.Object, NullLogger<BookingService>.Instance);

        // Act - Learner sends message
        var learnerMsg = await service.SendMessageAsync(learnerId, booking.Id, "Chào thầy, em có câu hỏi về Non-functional requirements", isExpert: false);

        // Act - Expert replies
        var expertMsg = await service.SendMessageAsync(expertId, booking.Id, "Chào em, ở mục NFR em cần chú ý chỉ số Availability 99.9%", isExpert: true);

        // Assert
        Assert.NotNull(learnerMsg);
        Assert.Equal("Learner", learnerMsg.SenderRole);
        Assert.NotNull(expertMsg);
        Assert.Equal("Expert", expertMsg.SenderRole);

        // Verify status moved to InProgress
        var updatedBooking = await db.ExpertBookings.FindAsync(booking.Id);
        Assert.Equal(BookingStatus.InProgress, updatedBooking!.Status);
        Assert.NotNull(updatedBooking.ActualStartTime);
    }

    [Fact]
    public async Task CancelBookingAsync_RefundsConsultationCredit()
    {
        // Arrange
        using var db = CreateInMemoryAssessmentDb();
        var bookingRepo = new BookingRepository(db);
        var mockAssessmentRepo = new Mock<BAVES.Api.Modules.Assessment.Repositories.Interfaces.IAssessmentRepository>();
        var mockEntitlementService = new Mock<IEntitlementService>();

        const int learnerId = 10;
        const int expertId = 99;

        var reviewReq = new ReviewRequest
        {
            Id = 2,
            SubmissionId = 5,
            LearnerId = learnerId,
            AssignedReviewerId = expertId,
            Status = ReviewRequestStatus.Completed
        };
        db.ReviewRequests.Add(reviewReq);

        var booking = new ExpertBooking
        {
            ReviewRequestId = reviewReq.Id,
            SubmissionId = 5,
            LearnerId = learnerId,
            ExpertId = expertId,
            Status = BookingStatus.Pending
        };
        db.ExpertBookings.Add(booking);
        await db.SaveChangesAsync();

        var service = new BookingService(bookingRepo, mockAssessmentRepo.Object, mockEntitlementService.Object, NullLogger<BookingService>.Instance);

        // Act
        var result = await service.CancelBookingAsync(learnerId, booking.Id, "Học viên bận đột xuất", isExpert: false);

        // Assert
        Assert.Equal(BookingStatus.Cancelled, result.Status);

        // Verify refund credit was called
        mockEntitlementService.Verify(s => s.RefundConsultationCreditAsync(learnerId, booking.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBookingAsync_UnassignedReviewer_ThrowsInvalidOperationException()
    {
        using var db = CreateInMemoryAssessmentDb();
        var bookingRepo = new BookingRepository(db);
        var mockAssessmentRepo = new Mock<BAVES.Api.Modules.Assessment.Repositories.Interfaces.IAssessmentRepository>();
        var mockEntitlementService = new Mock<IEntitlementService>();

        const int learnerId = 10;
        const int reviewRequestId = 50;

        var reviewReqNoExpert = new ReviewRequest
        {
            Id = reviewRequestId,
            SubmissionId = 12,
            LearnerId = learnerId,
            AssignedReviewerId = null, // No expert assigned
            Status = ReviewRequestStatus.Completed
        };

        mockAssessmentRepo.Setup(r => r.GetReviewRequestByIdAsync(reviewRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviewReqNoExpert);

        var service = new BookingService(bookingRepo, mockAssessmentRepo.Object, mockEntitlementService.Object, NullLogger<BookingService>.Instance);
        var request = new CreateExpertBookingRequest(reviewRequestId, null, null, null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateBookingAsync(learnerId, request));
        Assert.Contains("chưa có Chuyên gia", ex.Message);
    }

    [Fact]
    public async Task CreateBookingAsync_DuplicateBooking_ThrowsInvalidOperationException()
    {
        using var db = CreateInMemoryAssessmentDb();
        var bookingRepo = new BookingRepository(db);
        var mockAssessmentRepo = new Mock<BAVES.Api.Modules.Assessment.Repositories.Interfaces.IAssessmentRepository>();
        var mockEntitlementService = new Mock<IEntitlementService>();

        const int learnerId = 10;
        const int expertId = 99;
        const int reviewRequestId = 60;

        var reviewReq = new ReviewRequest
        {
            Id = reviewRequestId,
            SubmissionId = 15,
            LearnerId = learnerId,
            AssignedReviewerId = expertId,
            Status = ReviewRequestStatus.Completed
        };
        db.ReviewRequests.Add(reviewReq);

        // Đã có booking trước đó
        var existing = new ExpertBooking
        {
            ReviewRequestId = reviewRequestId,
            SubmissionId = 15,
            LearnerId = learnerId,
            ExpertId = expertId,
            Status = BookingStatus.Confirmed
        };
        db.ExpertBookings.Add(existing);
        await db.SaveChangesAsync();

        mockAssessmentRepo.Setup(r => r.GetReviewRequestByIdAsync(reviewRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviewReq);

        mockEntitlementService.Setup(s => s.GetConsultationCreditsAsync(learnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var service = new BookingService(bookingRepo, mockAssessmentRepo.Object, mockEntitlementService.Object, NullLogger<BookingService>.Instance);
        var request = new CreateExpertBookingRequest(reviewRequestId, null, null, "Đặt lần 2");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateBookingAsync(learnerId, request));
        Assert.Contains("đã được đặt lịch", ex.Message);
    }

    [Fact]
    public async Task SendMessageAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        using var db = CreateInMemoryAssessmentDb();
        var bookingRepo = new BookingRepository(db);
        var mockAssessmentRepo = new Mock<BAVES.Api.Modules.Assessment.Repositories.Interfaces.IAssessmentRepository>();
        var mockEntitlementService = new Mock<IEntitlementService>();

        var reviewReq = new ReviewRequest
        {
            Id = 70,
            SubmissionId = 20,
            LearnerId = 10,
            AssignedReviewerId = 99,
            Status = ReviewRequestStatus.Completed
        };
        db.ReviewRequests.Add(reviewReq);

        var booking = new ExpertBooking
        {
            ReviewRequestId = reviewReq.Id,
            SubmissionId = 20,
            LearnerId = 10,
            ExpertId = 99,
            Status = BookingStatus.Confirmed
        };
        db.ExpertBookings.Add(booking);
        await db.SaveChangesAsync();

        var service = new BookingService(bookingRepo, mockAssessmentRepo.Object, mockEntitlementService.Object, NullLogger<BookingService>.Instance);

        // User 999 là người lạ
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.SendMessageAsync(currentUserId: 999, booking.Id, "Hello", isExpert: false));
    }

    [Fact]
    public async Task SendMessageAsync_EmptyContent_ThrowsArgumentException()
    {
        using var db = CreateInMemoryAssessmentDb();
        var bookingRepo = new BookingRepository(db);
        var mockAssessmentRepo = new Mock<BAVES.Api.Modules.Assessment.Repositories.Interfaces.IAssessmentRepository>();
        var mockEntitlementService = new Mock<IEntitlementService>();

        var service = new BookingService(bookingRepo, mockAssessmentRepo.Object, mockEntitlementService.Object, NullLogger<BookingService>.Instance);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendMessageAsync(currentUserId: 10, bookingId: 1, content: "   ", isExpert: false));
    }

    [Fact]
    public async Task SendMessageAsync_CompletedBooking_ThrowsInvalidOperationException()
    {
        using var db = CreateInMemoryAssessmentDb();
        var bookingRepo = new BookingRepository(db);
        var mockAssessmentRepo = new Mock<BAVES.Api.Modules.Assessment.Repositories.Interfaces.IAssessmentRepository>();
        var mockEntitlementService = new Mock<IEntitlementService>();

        var reviewReq = new ReviewRequest
        {
            Id = 80,
            SubmissionId = 25,
            LearnerId = 10,
            AssignedReviewerId = 99,
            Status = ReviewRequestStatus.Completed
        };
        db.ReviewRequests.Add(reviewReq);

        var booking = new ExpertBooking
        {
            ReviewRequestId = reviewReq.Id,
            SubmissionId = 25,
            LearnerId = 10,
            ExpertId = 99,
            Status = BookingStatus.Completed // Đã xong
        };
        db.ExpertBookings.Add(booking);
        await db.SaveChangesAsync();

        var service = new BookingService(bookingRepo, mockAssessmentRepo.Object, mockEntitlementService.Object, NullLogger<BookingService>.Instance);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SendMessageAsync(currentUserId: 10, booking.Id, "Tin nhắn sau khi kết thúc", isExpert: false));

        Assert.Contains("đã kết thúc", ex.Message);
    }

    [Fact]
    public async Task UpdateBookingStatusAsync_ByExpert_TransitionsStatusAndRecordsNotes()
    {
        using var db = CreateInMemoryAssessmentDb();
        var bookingRepo = new BookingRepository(db);
        var mockAssessmentRepo = new Mock<BAVES.Api.Modules.Assessment.Repositories.Interfaces.IAssessmentRepository>();
        var mockEntitlementService = new Mock<IEntitlementService>();

        const int expertId = 99;

        var reviewReq = new ReviewRequest
        {
            Id = 90,
            SubmissionId = 30,
            LearnerId = 10,
            AssignedReviewerId = expertId,
            Status = ReviewRequestStatus.Completed
        };
        db.ReviewRequests.Add(reviewReq);

        var booking = new ExpertBooking
        {
            ReviewRequestId = reviewReq.Id,
            SubmissionId = 30,
            LearnerId = 10,
            ExpertId = expertId,
            Status = BookingStatus.Pending
        };
        db.ExpertBookings.Add(booking);
        await db.SaveChangesAsync();

        var service = new BookingService(bookingRepo, mockAssessmentRepo.Object, mockEntitlementService.Object, NullLogger<BookingService>.Instance);

        // 1. Expert chuyển sang InProgress
        var inProgressResult = await service.UpdateBookingStatusAsync(expertId, booking.Id, new UpdateBookingStatusRequest(BookingStatus.InProgress, null), isExpert: true);
        Assert.Equal(BookingStatus.InProgress, inProgressResult.Status);
        Assert.NotNull(inProgressResult.ActualStartTime);

        // 2. Expert hoàn tất và ghi summary note
        var completeResult = await service.UpdateBookingStatusAsync(expertId, booking.Id, new UpdateBookingStatusRequest(BookingStatus.Completed, "Buổi trao đổi rất tốt, học viên nắm vững BABOK."), isExpert: true);
        Assert.Equal(BookingStatus.Completed, completeResult.Status);
        Assert.NotNull(completeResult.ActualEndTime);
        Assert.Equal("Buổi trao đổi rất tốt, học viên nắm vững BABOK.", completeResult.ExpertSummaryNote);
    }

    [Fact]
    public async Task CancelBookingAsync_AlreadyCompleted_ThrowsInvalidOperationException()
    {
        using var db = CreateInMemoryAssessmentDb();
        var bookingRepo = new BookingRepository(db);
        var mockAssessmentRepo = new Mock<BAVES.Api.Modules.Assessment.Repositories.Interfaces.IAssessmentRepository>();
        var mockEntitlementService = new Mock<IEntitlementService>();

        var reviewReq = new ReviewRequest
        {
            Id = 100,
            SubmissionId = 35,
            LearnerId = 10,
            AssignedReviewerId = 99,
            Status = ReviewRequestStatus.Completed
        };
        db.ReviewRequests.Add(reviewReq);

        var booking = new ExpertBooking
        {
            ReviewRequestId = reviewReq.Id,
            SubmissionId = 35,
            LearnerId = 10,
            ExpertId = 99,
            Status = BookingStatus.Completed
        };
        db.ExpertBookings.Add(booking);
        await db.SaveChangesAsync();

        var service = new BookingService(bookingRepo, mockAssessmentRepo.Object, mockEntitlementService.Object, NullLogger<BookingService>.Instance);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CancelBookingAsync(currentUserId: 10, booking.Id, null, isExpert: false));

        Assert.Contains("Không thể hủy", ex.Message);
    }

    [Fact]
    public async Task GetBookingChatDossierAsync_ReturnsBookingWithHumanReviewAndMessages()
    {
        using var db = CreateInMemoryAssessmentDb();
        var bookingRepo = new BookingRepository(db);
        var mockAssessmentRepo = new Mock<BAVES.Api.Modules.Assessment.Repositories.Interfaces.IAssessmentRepository>();
        var mockEntitlementService = new Mock<IEntitlementService>();

        const int submissionId = 40;
        const int learnerId = 10;
        const int expertId = 99;

        var reviewReq = new ReviewRequest
        {
            Id = 110,
            SubmissionId = submissionId,
            LearnerId = learnerId,
            AssignedReviewerId = expertId,
            Status = ReviewRequestStatus.Completed
        };
        db.ReviewRequests.Add(reviewReq);

        var booking = new ExpertBooking
        {
            ReviewRequestId = reviewReq.Id,
            SubmissionId = submissionId,
            LearnerId = learnerId,
            ExpertId = expertId,
            Status = BookingStatus.InProgress
        };
        db.ExpertBookings.Add(booking);
        await db.SaveChangesAsync();

        // Thêm 1 tin nhắn
        await bookingRepo.AddMessageAsync(new ConsultationMessage
        {
            BookingId = booking.Id,
            SenderId = learnerId,
            SenderRole = "Learner",
            Content = "Em có câu hỏi về Use Case",
            SentAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var mockReviewDto = new HumanReviewDto(
            Id: 55,
            SubmissionId: submissionId,
            RubricId: 1,
            ReviewerId: expertId,
            FinalScore: 85.5m,
            OverallFeedback: "Bài làm rất tốt",
            CriterionScores: new List<HumanReviewCriterionScoreDto>(),
            ReviewedAt: DateTimeOffset.UtcNow
        );

        mockAssessmentRepo.Setup(r => r.GetHumanReviewDtoBySubmissionIdAsync(submissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockReviewDto);

        var service = new BookingService(bookingRepo, mockAssessmentRepo.Object, mockEntitlementService.Object, NullLogger<BookingService>.Instance);

        // Act
        var dossier = await service.GetBookingChatDossierAsync(learnerId, booking.Id, isExpert: false);

        // Assert
        Assert.NotNull(dossier);
        Assert.NotNull(dossier.ReviewDossier);
        Assert.Equal(85.5m, dossier.ReviewDossier.FinalScore);
        Assert.Single(dossier.Messages);
        Assert.Equal("Em có câu hỏi về Use Case", dossier.Messages[0].Content);
    }
}
