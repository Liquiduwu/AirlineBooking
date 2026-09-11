using AirlineBooking.Application.Common.Exceptions;
using AirlineBooking.Application.Common.Interfaces;
using AirlineBooking.Application.Features.Flights;
using AirlineBooking.Domain.Entities;
using Moq;
using Xunit;

namespace AirlineBooking.UnitTests.Features.Flights;

public class FlightServiceTests
{
    private readonly Mock<IFlightRepository> _flightRepository = new();
    private readonly Mock<IAircraftRepository> _aircraftRepository = new();
    private readonly Mock<ISeatRepository> _seatRepository = new();
    private readonly Mock<IFareClassRepository> _fareClassRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly FlightService _sut;

    public FlightServiceTests()
    {
        _sut = new FlightService(
            _flightRepository.Object,
            _aircraftRepository.Object,
            _seatRepository.Object,
            _fareClassRepository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task CreateAsync_ArrivalBeforeDeparture_ThrowsValidationAppException()
    {
        var aircraft = new Aircraft { Id = 1, RegistrationNumber = "A6-JZR", Model = "A320", TotalSeats = 12 };
        _aircraftRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(aircraft);

        var departure = new DateTime(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc);
        var request = new CreateFlightRequest("J9 101", "KWI", "DXB", departure, departure.AddMinutes(-30), 1, 85m);

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_UnknownAircraft_ThrowsNotFoundException()
    {
        _aircraftRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((Aircraft?)null);

        var departure = new DateTime(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc);
        var request = new CreateFlightRequest("J9 101", "KWI", "DXB", departure, departure.AddHours(2), 999, 85m);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsFlightAndReturnsDto()
    {
        var aircraft = new Aircraft { Id = 1, RegistrationNumber = "A6-JZR", Model = "Airbus A320neo", TotalSeats = 12 };
        _aircraftRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(aircraft);

        var departure = new DateTime(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc);
        var request = new CreateFlightRequest("J9 101", "kwi", "dxb", departure, departure.AddHours(2), 1, 85m);

        var result = await _sut.CreateAsync(request);

        Assert.Equal("KWI", result.OriginAirportCode);
        Assert.Equal("DXB", result.DestinationAirportCode);
        Assert.Equal("Airbus A320neo", result.AircraftModel);
        _flightRepository.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Flight>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
