using AirlineBooking.Domain.Entities;

namespace AirlineBooking.Application.Common.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
    Task<Booking?> GetByReferenceAsync(string bookingReference, CancellationToken cancellationToken = default);

    Task<Booking?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    Task<List<Booking>> ListWithDetailsAsync(CancellationToken cancellationToken = default);

    /// <summary>True if an active (Held or Confirmed) booking already occupies this seat on
    /// this flight. Used as an app-level pre-check before insert; the database's filtered
    /// unique index is the authoritative guard against a race between two concurrent holds.</summary>
    Task<bool> IsSeatTakenAsync(int flightId, int seatId, CancellationToken cancellationToken = default);

    Task<bool> ReferenceExistsAsync(string bookingReference, CancellationToken cancellationToken = default);

    /// <summary>Held bookings whose HeldUntilUtc has passed - picked up by the hold-expiry
    /// background sweep.</summary>
    Task<List<Booking>> GetExpiredHoldsAsync(DateTime asOfUtc, CancellationToken cancellationToken = default);
}
