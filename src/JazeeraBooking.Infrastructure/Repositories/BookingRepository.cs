using JazeeraBooking.Application.Common.Interfaces;
using JazeeraBooking.Domain.Entities;
using JazeeraBooking.Domain.Enums;
using JazeeraBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JazeeraBooking.Infrastructure.Repositories;

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(JazeeraBookingDbContext context) : base(context)
    {
    }

    public async Task<Booking?> GetByReferenceAsync(string bookingReference, CancellationToken cancellationToken = default) =>
        await DbSet.Include(b => b.Passenger)
            .Include(b => b.Flight)
            .Include(b => b.Seat)
            .Include(b => b.FareClass)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BookingReference == bookingReference, cancellationToken);

    public async Task<Booking?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await DbSet.Include(b => b.Passenger)
            .Include(b => b.Flight)
            .Include(b => b.Seat)
            .Include(b => b.FareClass)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<List<Booking>> ListWithDetailsAsync(CancellationToken cancellationToken = default) =>
        await DbSet.Include(b => b.Passenger)
            .Include(b => b.Flight)
            .Include(b => b.Seat)
            .Include(b => b.FareClass)
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedUtc)
            .ToListAsync(cancellationToken);

    public async Task<bool> IsSeatTakenAsync(int flightId, int seatId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().AnyAsync(
            b => b.FlightId == flightId
                 && b.SeatId == seatId
                 && (b.Status == BookingStatus.Held || b.Status == BookingStatus.Confirmed),
            cancellationToken);

    public async Task<bool> ReferenceExistsAsync(string bookingReference, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().AnyAsync(b => b.BookingReference == bookingReference, cancellationToken);

    public async Task<List<Booking>> GetExpiredHoldsAsync(DateTime asOfUtc, CancellationToken cancellationToken = default) =>
        await DbSet.Where(
                b => b.Status == BookingStatus.Held
                     && b.HeldUntilUtc != null
                     && b.HeldUntilUtc < asOfUtc)
            .ToListAsync(cancellationToken);
}
