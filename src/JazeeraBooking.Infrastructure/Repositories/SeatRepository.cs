using JazeeraBooking.Application.Common.Interfaces;
using JazeeraBooking.Application.Common.Models;
using JazeeraBooking.Domain.Entities;
using JazeeraBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JazeeraBooking.Infrastructure.Repositories;

public class SeatRepository : Repository<Seat>, ISeatRepository
{
    public SeatRepository(JazeeraBookingDbContext context) : base(context)
    {
    }

    public async Task<List<SeatAvailabilityResult>> GetSeatAvailabilityAsync(int flightId, CancellationToken cancellationToken = default) =>
        // FromSqlInterpolated (not FromSqlRaw + string concatenation) parameterizes flightId
        // as a real SqlParameter under the hood, so this is safe against SQL injection even
        // though it looks like an inline string.
        await Context.SeatAvailabilityResults
            .FromSqlInterpolated($"EXEC dbo.sp_GetSeatAvailability @FlightId = {flightId}")
            .ToListAsync(cancellationToken);

    public async Task<List<Seat>> GetByAircraftIdAsync(int aircraftId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking()
            .Where(s => s.AircraftId == aircraftId)
            .OrderBy(s => s.SeatNumber)
            .ToListAsync(cancellationToken);
}
