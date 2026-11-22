using BAVES.Api.Modules.Assessment.Data;
using BAVES.Api.Modules.Assessment.Models.Entities;
using BAVES.Api.Modules.Assessment.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Assessment.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AssessmentDbContext _db;

    public BookingRepository(AssessmentDbContext db)
    {
        _db = db;
    }

    public async Task<ExpertBooking?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.ExpertBookings
            .Include(b => b.ReviewRequest)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<ExpertBooking?> GetByIdWithMessagesAsync(int id, CancellationToken ct = default)
    {
        return await _db.ExpertBookings
            .Include(b => b.ReviewRequest)
            .Include(b => b.Messages.OrderBy(m => m.SentAt))
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<ExpertBooking?> GetByReviewRequestIdAsync(int reviewRequestId, CancellationToken ct = default)
    {
        return await _db.ExpertBookings
            .FirstOrDefaultAsync(b => b.ReviewRequestId == reviewRequestId, ct);
    }

    public async Task<List<ExpertBooking>> GetByLearnerIdAsync(int learnerId, CancellationToken ct = default)
    {
        return await _db.ExpertBookings
            .Where(b => b.LearnerId == learnerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<ExpertBooking>> GetByExpertIdAsync(int expertId, CancellationToken ct = default)
    {
        return await _db.ExpertBookings
            .Where(b => b.ExpertId == expertId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddBookingAsync(ExpertBooking booking, CancellationToken ct = default)
    {
        await _db.ExpertBookings.AddAsync(booking, ct);
    }

    public Task UpdateBookingAsync(ExpertBooking booking, CancellationToken ct = default)
    {
        _db.ExpertBookings.Update(booking);
        return Task.CompletedTask;
    }

    public async Task AddMessageAsync(ConsultationMessage message, CancellationToken ct = default)
    {
        await _db.ConsultationMessages.AddAsync(message, ct);
    }

    public async Task<List<ConsultationMessage>> GetMessagesAsync(int bookingId, CancellationToken ct = default)
    {
        return await _db.ConsultationMessages
            .Where(m => m.BookingId == bookingId)
            .OrderBy(m => m.SentAt)
            .ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return _db.SaveChangesAsync(ct);
    }
}
