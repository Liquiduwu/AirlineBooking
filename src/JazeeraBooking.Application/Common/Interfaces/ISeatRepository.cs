using JazeeraBooking.Application.Common.Models;
using JazeeraBooking.Domain.Entities;

namespace JazeeraBooking.Application.Common.Interfaces;

public interface ISeatRepository : IRepository<Seat>
{
    /// <summary>Calls sp_GetSeatAvailability - a full seat-map-with-availability query joined
    /// against active bookings for the given flight, computed in SQL Server rather than pulled
    /// into the app and filtered in memory.</summary>
    Task<List<SeatAvailabilityResult>> GetSeatAvailabilityAsync(int flightId, CancellationToken cancellationToken = default);

    Task<List<Seat>> GetByAircraftIdAsync(int aircraftId, CancellationToken cancellationToken = default);
}
