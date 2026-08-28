using JazeeraBooking.Application.Common.Models;
using JazeeraBooking.Domain.Entities;

namespace JazeeraBooking.Application.Common.Interfaces;

public interface IFlightRepository : IRepository<Flight>
{
    Task<Flight?> GetWithAircraftAsync(int id, CancellationToken cancellationToken = default);

    Task<List<Flight>> SearchAsync(
        string? originAirportCode,
        string? destinationAirportCode,
        DateOnly? departureDate,
        CancellationToken cancellationToken = default);

    /// <summary>Calls sp_CalculateFare - computes price server-side in SQL (base fare *
    /// multiplier + a load-factor surcharge that rises as the flight fills up) instead of
    /// duplicating that pricing rule in C#, so the DB and the app can never disagree on price.</summary>
    Task<FareQuoteResult?> GetFareQuoteAsync(int flightId, int fareClassId, CancellationToken cancellationToken = default);
}
