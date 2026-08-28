using JazeeraBooking.Application.Common.Interfaces;
using JazeeraBooking.Application.Common.Models;
using JazeeraBooking.Domain.Entities;
using JazeeraBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JazeeraBooking.Infrastructure.Repositories;

public class FlightRepository : Repository<Flight>, IFlightRepository
{
    public FlightRepository(JazeeraBookingDbContext context) : base(context)
    {
    }

    public async Task<Flight?> GetWithAircraftAsync(int id, CancellationToken cancellationToken = default) =>
        await DbSet.Include(f => f.Aircraft)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<List<Flight>> SearchAsync(
        string? originAirportCode,
        string? destinationAirportCode,
        DateOnly? departureDate,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Include(f => f.Aircraft).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(originAirportCode))
        {
            query = query.Where(f => f.OriginAirportCode == originAirportCode.ToUpperInvariant());
        }

        if (!string.IsNullOrWhiteSpace(destinationAirportCode))
        {
            query = query.Where(f => f.DestinationAirportCode == destinationAirportCode.ToUpperInvariant());
        }

        if (departureDate.HasValue)
        {
            var start = departureDate.Value.ToDateTime(TimeOnly.MinValue);
            var end = start.AddDays(1);
            query = query.Where(f => f.DepartureUtc >= start && f.DepartureUtc < end);
        }

        return await query.OrderBy(f => f.DepartureUtc).ToListAsync(cancellationToken);
    }

    public async Task<FareQuoteResult?> GetFareQuoteAsync(int flightId, int fareClassId, CancellationToken cancellationToken = default)
    {
        // Stored-procedure result sets can't be composed with further LINQ operators
        // (no Where/OrderBy/FirstOrDefault translation - EF just executes the proc as-is), so
        // we materialize the single-row result set and take the first row client-side.
        var results = await Context.FareQuoteResults
            .FromSqlInterpolated($"EXEC dbo.sp_CalculateFare @FlightId = {flightId}, @FareClassId = {fareClassId}")
            .ToListAsync(cancellationToken);

        return results.FirstOrDefault();
    }
}
