using AirlineBooking.Application.Common.Exceptions;
using AirlineBooking.Application.Common.Interfaces;
using AirlineBooking.Application.Common.Models;
using AirlineBooking.Application.Features.Bookings;
using AirlineBooking.Domain.Entities;
using AirlineBooking.Domain.Enums;
using Moq;
using Xunit;

namespace AirlineBooking.UnitTests.Features.Bookings;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _bookingRepository = new();
    private readonly Mock<IPassengerRepository> _passengerRepository = new();
    private readonly Mock<IFlightRepository> _flightRepository = new();
    private readonly Mock<ISeatRepository> _seatRepository = new();
    private readonly Mock<IFareClassRepository> _fareClassRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly BookingService _sut;

    public BookingServiceTests()
    {
        _sut = new BookingService(
            _bookingRepository.Object,
            _passengerRepository.Object,
            _flightRepository.Object,
            _seatRepository.Object,
            _fareClassRepository.Object,
            _unitOfWork.Object);
    }

    private static Passenger MakePassenger(int id = 1) =>
        new() { Id = id, FirstName = "Yousef", LastName = "Ali", Email = "y@example.com", PassportNumber = "P1", DateOfBirth = new DateOnly(1995, 1, 1) };

    private static Flight MakeFlight(int id = 1, int aircraftId = 1) =>
        new() { Id = id, FlightNumber = "J9 101", OriginAirportCode = "KWI", DestinationAirportCode = "DXB", AircraftId = aircraftId, BaseFare = 85m };

    private static Seat MakeSeat(int id = 1, int aircraftId = 1) =>
        new() { Id = id, AircraftId = aircraftId, SeatNumber = "3C", CabinClass = CabinClass.Economy };

    private static FareClass MakeFareClass(int id = 1) =>
        new() { Id = id, Code = "Y", Name = "Economy Saver", CabinClass = CabinClass.Economy, PriceMultiplier = 1.0m };

    [Fact]
    public async Task CreateHoldAsync_ValidRequest_CreatesHeldBookingWithQuotedFare()
    {
        var passenger = MakePassenger();
        var flight = MakeFlight();
        var seat = MakeSeat();
        var fareClass = MakeFareClass();

        _passengerRepository.Setup(r => r.GetByIdAsync(passenger.Id, It.IsAny<CancellationToken>())).ReturnsAsync(passenger);
        _flightRepository.Setup(r => r.GetWithAircraftAsync(flight.Id, It.IsAny<CancellationToken>())).ReturnsAsync(flight);
        _seatRepository.Setup(r => r.GetByIdAsync(seat.Id, It.IsAny<CancellationToken>())).ReturnsAsync(seat);
        _fareClassRepository.Setup(r => r.GetByIdAsync(fareClass.Id, It.IsAny<CancellationToken>())).ReturnsAsync(fareClass);
        _bookingRepository.Setup(r => r.IsSeatTakenAsync(flight.Id, seat.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _bookingRepository.Setup(r => r.ReferenceExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _flightRepository.Setup(r => r.GetFareQuoteAsync(flight.Id, fareClass.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FareQuoteResult { BaseFare = 85m, PriceMultiplier = 1m, LoadFactorSurcharge = 0m, TotalFare = 85m });

        var request = new CreateBookingRequest(passenger.Id, flight.Id, seat.Id, fareClass.Id);
        var result = await _sut.CreateHoldAsync(request);

        Assert.Equal(BookingStatus.Held, result.Status);
        Assert.Equal(85m, result.FareAmount);
        Assert.Equal(6, result.BookingReference.Length);
        Assert.NotNull(result.HeldUntilUtc);
        _bookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateHoldAsync_SeatAlreadyTaken_ThrowsConflictException()
    {
        var passenger = MakePassenger();
        var flight = MakeFlight();
        var seat = MakeSeat();
        var fareClass = MakeFareClass();

        _passengerRepository.Setup(r => r.GetByIdAsync(passenger.Id, It.IsAny<CancellationToken>())).ReturnsAsync(passenger);
        _flightRepository.Setup(r => r.GetWithAircraftAsync(flight.Id, It.IsAny<CancellationToken>())).ReturnsAsync(flight);
        _seatRepository.Setup(r => r.GetByIdAsync(seat.Id, It.IsAny<CancellationToken>())).ReturnsAsync(seat);
        _fareClassRepository.Setup(r => r.GetByIdAsync(fareClass.Id, It.IsAny<CancellationToken>())).ReturnsAsync(fareClass);
        _bookingRepository.Setup(r => r.IsSeatTakenAsync(flight.Id, seat.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var request = new CreateBookingRequest(passenger.Id, flight.Id, seat.Id, fareClass.Id);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.CreateHoldAsync(request));
        _bookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateHoldAsync_SeatNotOnFlightAircraft_ThrowsValidationAppException()
    {
        var passenger = MakePassenger();
        var flight = MakeFlight(aircraftId: 1);
        var seat = MakeSeat(aircraftId: 2); // belongs to a different aircraft
        var fareClass = MakeFareClass();

        _passengerRepository.Setup(r => r.GetByIdAsync(passenger.Id, It.IsAny<CancellationToken>())).ReturnsAsync(passenger);
        _flightRepository.Setup(r => r.GetWithAircraftAsync(flight.Id, It.IsAny<CancellationToken>())).ReturnsAsync(flight);
        _seatRepository.Setup(r => r.GetByIdAsync(seat.Id, It.IsAny<CancellationToken>())).ReturnsAsync(seat);
        _fareClassRepository.Setup(r => r.GetByIdAsync(fareClass.Id, It.IsAny<CancellationToken>())).ReturnsAsync(fareClass);

        var request = new CreateBookingRequest(passenger.Id, flight.Id, seat.Id, fareClass.Id);

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.CreateHoldAsync(request));
    }

    [Fact]
    public async Task CreateHoldAsync_PassengerNotFound_ThrowsNotFoundException()
    {
        _passengerRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((Passenger?)null);

        var request = new CreateBookingRequest(999, 1, 1, 1);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateHoldAsync(request));
    }

    private static Booking MakeHeldBooking(DateTime? heldUntilUtc = null) => new()
    {
        Id = 1,
        BookingReference = "ABC123",
        Status = BookingStatus.Held,
        FareAmount = 85m,
        HeldUntilUtc = heldUntilUtc ?? DateTime.UtcNow.AddMinutes(10),
        Passenger = MakePassenger(),
        Flight = MakeFlight(),
        Seat = MakeSeat(),
        FareClass = MakeFareClass()
    };

    [Fact]
    public async Task ConfirmAsync_HeldBookingWithinWindow_ConfirmsBooking()
    {
        var booking = MakeHeldBooking();
        _bookingRepository.Setup(r => r.GetByReferenceAsync(booking.BookingReference, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        _bookingRepository.Setup(r => r.GetWithDetailsAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var result = await _sut.ConfirmAsync(booking.BookingReference);

        Assert.Equal(BookingStatus.Confirmed, result.Status);
        Assert.Null(result.HeldUntilUtc);
        Assert.NotNull(result.ConfirmedUtc);
    }

    [Fact]
    public async Task ConfirmAsync_HoldHasExpired_ThrowsConflictExceptionAndMarksExpired()
    {
        var booking = MakeHeldBooking(heldUntilUtc: DateTime.UtcNow.AddMinutes(-1));
        _bookingRepository.Setup(r => r.GetByReferenceAsync(booking.BookingReference, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        _bookingRepository.Setup(r => r.GetWithDetailsAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.ConfirmAsync(booking.BookingReference));

        Assert.Equal(BookingStatus.Expired, booking.Status);
    }

    [Fact]
    public async Task ConfirmAsync_AlreadyConfirmed_ThrowsConflictException()
    {
        var booking = MakeHeldBooking();
        booking.Status = BookingStatus.Confirmed;
        booking.HeldUntilUtc = null;
        _bookingRepository.Setup(r => r.GetByReferenceAsync(booking.BookingReference, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        _bookingRepository.Setup(r => r.GetWithDetailsAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.ConfirmAsync(booking.BookingReference));
    }

    [Fact]
    public async Task CancelAsync_HeldBooking_CancelsAndFreesSeat()
    {
        var booking = MakeHeldBooking();
        _bookingRepository.Setup(r => r.GetByReferenceAsync(booking.BookingReference, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        _bookingRepository.Setup(r => r.GetWithDetailsAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var result = await _sut.CancelAsync(booking.BookingReference);

        Assert.Equal(BookingStatus.Cancelled, result.Status);
        Assert.NotNull(result.CancelledUtc);
    }

    [Fact]
    public async Task CancelAsync_AlreadyCancelled_ThrowsConflictException()
    {
        var booking = MakeHeldBooking();
        booking.Status = BookingStatus.Cancelled;
        _bookingRepository.Setup(r => r.GetByReferenceAsync(booking.BookingReference, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        _bookingRepository.Setup(r => r.GetWithDetailsAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.CancelAsync(booking.BookingReference));
    }

    [Fact]
    public async Task GetByReferenceAsync_UnknownReference_ThrowsNotFoundException()
    {
        _bookingRepository.Setup(r => r.GetByReferenceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Booking?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByReferenceAsync("ZZZZZZ"));
    }
}
