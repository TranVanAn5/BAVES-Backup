using BAVES.Api.Modules.Assessment.Models.DTOs;

namespace BAVES.Api.Modules.Assessment.Services.Interfaces;

public interface IBookingService
{
    Task<ExpertBookingDto> CreateBookingAsync(int learnerId, CreateExpertBookingRequest request, CancellationToken ct = default);
    Task<List<ExpertBookingDto>> GetLearnerBookingsAsync(int learnerId, CancellationToken ct = default);
    Task<List<ExpertBookingDto>> GetExpertBookingsAsync(int expertId, CancellationToken ct = default);
    Task<BookingChatDossierDto> GetBookingChatDossierAsync(int currentUserId, int bookingId, bool isExpert, CancellationToken ct = default);
    Task<ConsultationMessageDto> SendMessageAsync(int currentUserId, int bookingId, string content, bool isExpert, CancellationToken ct = default);
    Task<ExpertBookingDto> UpdateBookingStatusAsync(int currentUserId, int bookingId, UpdateBookingStatusRequest request, bool isExpert, CancellationToken ct = default);
    Task<ExpertBookingDto> CancelBookingAsync(int currentUserId, int bookingId, string? cancelReason, bool isExpert, CancellationToken ct = default);
}
