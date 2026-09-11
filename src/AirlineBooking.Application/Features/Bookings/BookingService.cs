using AirlineBooking.Application.Common.Exceptions;
using AirlineBooking.Application.Common.Interfaces;
using AirlineBooking.Domain.Entities;
using AirlineBooking.Domain.Enums;

namespace AirlineBooking.Application.Features.Bookings;

public class BookingService : IBookingService
{
    // Real airline hold windows are typically 15-30 minutes for an agent to complete payment.
    private const int HoldDurationMinutes = 15;
    private const int MaxReferenceGenerationAttempts = 5;

    private readonly IBookingRepository _bookingRepository;
    private readonly IPassengerRepository _passengerRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IFareClassRepository _fareClassRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(
        IBookingRepository bookingRepository,
        IPassengerRepository passengerRepository,
        IFlightRepository flightRepository,
        ISeatRepository seatRepository,
        IFareClassRepository fareClassRepository,
        IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _passengerRepository = passengerRepository;
        _flightRepository = flightRepository;
        _seatRepository = seatRepository;
        _fareClassRepository = fareClassRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BookingDto> CreateHoldAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        var passenger = await _passengerRepository.GetByIdAsync(request.PassengerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Passenger), request.PassengerId);
        var flight = await _flightRepository.GetWithAircraftAsync(request.FlightId, cancellationToken)
            ?? throw new NotFoundException(nameof(Flight), request.FlightId);
        var seat = await _seatRepository.GetByIdAsync(request.SeatId, cancellationToken)
            ?? throw new NotFoundException(nameof(Seat), request.SeatId);
        var fareClass = await _fareClassRepository.GetByIdAsync(request.FareClassId, cancellationToken)
            ?? throw new NotFoundException(nameof(FareClass), request.FareClassId);

        if (seat.AircraftId != flight.AircraftId)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.SeatId)] = ["Seat does not belong to the aircraft operating this flight."]
            });
        }

        // App-level check gives a clean 409 in the common case; the database's filtered
        // unique index (see BookingConfiguration) is what actually prevents a double-booking
        // if two requests for the same seat land concurrently and both pass this check.
        if (await _bookingRepository.IsSeatTakenAsync(request.FlightId, request.SeatId, cancellationToken))
        {
            throw new ConflictException($"Seat {seat.SeatNumber} is not available on this flight.");
        }

        var quote = await _flightRepository.GetFareQuoteAsync(request.FlightId, request.FareClassId, cancellationToken)
            ?? throw new NotFoundException("Fare quote could not be computed for this flight/fare class combination.");

        var booking = new Booking
        {
            BookingReference = await GenerateUniqueReferenceAsync(cancellationToken),
            PassengerId = passenger.Id,
            FlightId = flight.Id,
            SeatId = seat.Id,
            FareClassId = fareClass.Id,
            Status = BookingStatus.Held,
            FareAmount = quote.TotalFare,
            HeldUntilUtc = DateTime.UtcNow.AddMinutes(HoldDurationMinutes)
        };

        await _bookingRepository.AddAsync(booking, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConflictException)
        {
            // Infrastructure already translated a unique-constraint violation into
            // ConflictException (see AirlineBookingDbContext.SaveChangesAsync) - we just give
            // it a more specific, seat-aware message here since we have that context.
            throw new ConflictException($"Seat {seat.SeatNumber} is not available on this flight.");
        }

        booking.Passenger = passenger;
        booking.Flight = flight;
        booking.Seat = seat;
        booking.FareClass = fareClass;
        return ToDto(booking);
    }

    public async Task<BookingDto> ConfirmAsync(string bookingReference, CancellationToken cancellationToken = default)
    {
        var booking = await GetTrackedByReferenceAsync(bookingReference, cancellationToken);

        if (booking.Status == BookingStatus.Held && booking.HeldUntilUtc < DateTime.UtcNow)
        {
            booking.Status = BookingStatus.Expired;
            booking.HeldUntilUtc = null;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new ConflictException("This hold has expired. Create a new booking.");
        }

        if (booking.Status != BookingStatus.Held)
        {
            throw new ConflictException($"Only Held bookings can be confirmed (current status: {booking.Status}).");
        }

        booking.Status = BookingStatus.Confirmed;
        booking.ConfirmedUtc = DateTime.UtcNow;
        booking.HeldUntilUtc = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(booking);
    }

    public async Task<BookingDto> CancelAsync(string bookingReference, CancellationToken cancellationToken = default)
    {
        var booking = await GetTrackedByReferenceAsync(bookingReference, cancellationToken);

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Expired)
        {
            throw new ConflictException($"Booking is already {booking.Status}.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledUtc = DateTime.UtcNow;
        booking.HeldUntilUtc = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(booking);
    }

    public async Task<BookingDto> GetByReferenceAsync(string bookingReference, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByReferenceAsync(bookingReference, cancellationToken)
            ?? throw new NotFoundException($"Booking with reference '{bookingReference}' was not found.");
        return ToDto(booking);
    }

    public async Task<List<BookingDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var bookings = await _bookingRepository.ListWithDetailsAsync(cancellationToken);
        return bookings.Select(ToDto).ToList();
    }

    private async Task<Booking> GetTrackedByReferenceAsync(string bookingReference, CancellationToken cancellationToken)
    {
        var untracked = await _bookingRepository.GetByReferenceAsync(bookingReference, cancellationToken)
            ?? throw new NotFoundException($"Booking with reference '{bookingReference}' was not found.");
        // GetByReferenceAsync reads with AsNoTracking (cheap for the common read-only lookup
        // path); state transitions need a tracked instance, so re-fetch by the now-known Id.
        return await _bookingRepository.GetWithDetailsAsync(untracked.Id, cancellationToken)
            ?? throw new NotFoundException($"Booking with reference '{bookingReference}' was not found.");
    }

    private async Task<string> GenerateUniqueReferenceAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxReferenceGenerationAttempts; attempt++)
        {
            var candidate = BookingReferenceGenerator.Generate();
            if (!await _bookingRepository.ReferenceExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Could not generate a unique booking reference after several attempts.");
    }

    private static BookingDto ToDto(Booking b) => new(
        b.Id, b.BookingReference,
        b.PassengerId, $"{b.Passenger.FirstName} {b.Passenger.LastName}",
        b.FlightId, b.Flight.FlightNumber,
        b.SeatId, b.Seat.SeatNumber,
        b.FareClassId, b.FareClass.Code,
        b.Status, b.FareAmount, b.HeldUntilUtc, b.ConfirmedUtc, b.CancelledUtc, b.CreatedUtc);
}
