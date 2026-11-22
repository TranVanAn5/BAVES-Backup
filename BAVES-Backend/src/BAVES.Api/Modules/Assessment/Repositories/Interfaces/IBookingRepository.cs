using BAVES.Api.Modules.Assessment.Models.Entities;

namespace BAVES.Api.Modules.Assessment.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<ExpertBooking?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ExpertBooking?> GetByIdWithMessagesAsync(int id, CancellationToken ct = default);
    Task<ExpertBooking?> GetByReviewRequestIdAsync(int reviewRequestId, CancellationToken ct = default);
    Task<List<ExpertBooking>> GetByLearnerIdAsync(int learnerId, CancellationToken ct = default);
    Task<List<ExpertBooking>> GetByExpertIdAsync(int expertId, CancellationToken ct = default);
    Task AddBookingAsync(ExpertBooking booking, CancellationToken ct = default);
    Task UpdateBookingAsync(ExpertBooking booking, CancellationToken ct = default);
    Task AddMessageAsync(ConsultationMessage message, CancellationToken ct = default);
    Task<List<ConsultationMessage>> GetMessagesAsync(int bookingId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
