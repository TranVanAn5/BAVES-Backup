using BAVES.Api.Modules.Assessment.Models.Enums;

namespace BAVES.Api.Modules.Assessment.Models.DTOs;

public record CreateExpertBookingRequest(
    int ReviewRequestId,
    DateTimeOffset? ScheduledStartTime,
    DateTimeOffset? ScheduledEndTime,
    string? LearnerNote
);

public record SendConsultationMessageRequest(
    string Content
);

public record UpdateBookingStatusRequest(
    BookingStatus Status,
    string? ExpertSummaryNote
);

public record ConsultationMessageDto(
    long Id,
    int BookingId,
    int SenderId,
    string SenderRole,
    string Content,
    DateTimeOffset SentAt,
    bool IsRead
);

public record ExpertBookingDto(
    int Id,
    int ReviewRequestId,
    int SubmissionId,
    int LearnerId,
    int ExpertId,
    BookingStatus Status,
    DateTimeOffset? ScheduledStartTime,
    DateTimeOffset? ScheduledEndTime,
    DateTimeOffset? ActualStartTime,
    DateTimeOffset? ActualEndTime,
    string? LearnerNote,
    string? ExpertSummaryNote,
    DateTimeOffset CreatedAt
);

public record BookingChatDossierDto(
    ExpertBookingDto Booking,
    HumanReviewDto? ReviewDossier,
    List<ConsultationMessageDto> Messages
);
