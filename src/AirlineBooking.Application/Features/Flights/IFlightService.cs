namespace AirlineBooking.Application.Features.Flights;

public interface IFlightService
{
    Task<List<FlightSummaryDto>> SearchAsync(FlightSearchRequest request, CancellationToken cancellationToken = default);
    Task<FlightSummaryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<FlightSummaryDto> CreateAsync(CreateFlightRequest request, CancellationToken cancellationToken = default);
    Task<SeatMapResponse> GetSeatMapAsync(int flightId, CancellationToken cancellationToken = default);
    Task<FareQuoteDto> GetFareQuoteAsync(int flightId, int fareClassId, CancellationToken cancellationToken = default);
}
