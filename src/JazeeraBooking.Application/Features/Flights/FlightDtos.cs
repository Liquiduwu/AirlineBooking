using JazeeraBooking.Domain.Enums;

namespace JazeeraBooking.Application.Features.Flights;

public record FlightSummaryDto(
    int Id,
    string FlightNumber,
    string OriginAirportCode,
    string DestinationAirportCode,
    DateTime DepartureUtc,
    DateTime ArrivalUtc,
    string AircraftModel,
    decimal BaseFare,
    FlightStatus Status);

/// <summary>Bound from query string ([FromQuery]) on GET /api/flights - all optional, so
/// callers can search by any combination of route legs and date.</summary>
public record FlightSearchRequest(string? OriginAirportCode, string? DestinationAirportCode, DateOnly? DepartureDate);

public record CreateFlightRequest(
    string FlightNumber,
    string OriginAirportCode,
    string DestinationAirportCode,
    DateTime DepartureUtc,
    DateTime ArrivalUtc,
    int AircraftId,
    decimal BaseFare);

public record SeatAvailabilityDto(int SeatId, string SeatNumber, CabinClass CabinClass, bool IsExitRow, bool IsAvailable);

public record SeatMapResponse(int FlightId, IReadOnlyList<SeatAvailabilityDto> Seats);

public record FareQuoteDto(decimal BaseFare, decimal PriceMultiplier, decimal LoadFactorSurcharge, decimal TotalFare);
