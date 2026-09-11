using AirlineBooking.Application.Common.Exceptions;
using AirlineBooking.Application.Common.Interfaces;
using AirlineBooking.Domain.Entities;

namespace AirlineBooking.Application.Features.Flights;

public class FlightService : IFlightService
{
    private readonly IFlightRepository _flightRepository;
    private readonly IAircraftRepository _aircraftRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IFareClassRepository _fareClassRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FlightService(
        IFlightRepository flightRepository,
        IAircraftRepository aircraftRepository,
        ISeatRepository seatRepository,
        IFareClassRepository fareClassRepository,
        IUnitOfWork unitOfWork)
    {
        _flightRepository = flightRepository;
        _aircraftRepository = aircraftRepository;
        _seatRepository = seatRepository;
        _fareClassRepository = fareClassRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<FlightSummaryDto>> SearchAsync(FlightSearchRequest request, CancellationToken cancellationToken = default)
    {
        var flights = await _flightRepository.SearchAsync(
            request.OriginAirportCode, request.DestinationAirportCode, request.DepartureDate, cancellationToken);
        return flights.Select(ToDto).ToList();
    }

    public async Task<FlightSummaryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var flight = await _flightRepository.GetWithAircraftAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Flight), id);
        return ToDto(flight);
    }

    public async Task<FlightSummaryDto> CreateAsync(CreateFlightRequest request, CancellationToken cancellationToken = default)
    {
        var aircraft = await _aircraftRepository.GetByIdAsync(request.AircraftId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Aircraft), request.AircraftId);

        if (request.ArrivalUtc <= request.DepartureUtc)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.ArrivalUtc)] = ["Arrival time must be after departure time."]
            });
        }

        var flight = new Flight
        {
            FlightNumber = request.FlightNumber,
            OriginAirportCode = request.OriginAirportCode.ToUpperInvariant(),
            DestinationAirportCode = request.DestinationAirportCode.ToUpperInvariant(),
            DepartureUtc = request.DepartureUtc,
            ArrivalUtc = request.ArrivalUtc,
            AircraftId = request.AircraftId,
            BaseFare = request.BaseFare
        };

        await _flightRepository.AddAsync(flight, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        flight.Aircraft = aircraft;
        return ToDto(flight);
    }

    public async Task<SeatMapResponse> GetSeatMapAsync(int flightId, CancellationToken cancellationToken = default)
    {
        _ = await _flightRepository.GetByIdAsync(flightId, cancellationToken)
            ?? throw new NotFoundException(nameof(Flight), flightId);

        var availability = await _seatRepository.GetSeatAvailabilityAsync(flightId, cancellationToken);
        var seats = availability
            .Select(a => new SeatAvailabilityDto(a.SeatId, a.SeatNumber, a.CabinClass, a.IsExitRow, a.IsAvailable))
            .ToList();

        return new SeatMapResponse(flightId, seats);
    }

    public async Task<FareQuoteDto> GetFareQuoteAsync(int flightId, int fareClassId, CancellationToken cancellationToken = default)
    {
        _ = await _flightRepository.GetByIdAsync(flightId, cancellationToken)
            ?? throw new NotFoundException(nameof(Flight), flightId);
        _ = await _fareClassRepository.GetByIdAsync(fareClassId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.FareClass), fareClassId);

        var quote = await _flightRepository.GetFareQuoteAsync(flightId, fareClassId, cancellationToken)
            ?? throw new NotFoundException("Fare quote could not be computed for this flight/fare class combination.");

        return new FareQuoteDto(quote.BaseFare, quote.PriceMultiplier, quote.LoadFactorSurcharge, quote.TotalFare);
    }

    private static FlightSummaryDto ToDto(Flight f) => new(
        f.Id, f.FlightNumber, f.OriginAirportCode, f.DestinationAirportCode,
        f.DepartureUtc, f.ArrivalUtc, f.Aircraft.Model, f.BaseFare, f.Status);
}
