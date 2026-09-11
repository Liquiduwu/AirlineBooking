using AirlineBooking.Application.Features.Flights;
using AirlineBooking.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirlineBooking.Api.Controllers;

/// <summary>Flight search, detail, seat maps, and fare quotes.</summary>
[ApiController]
[Route("api/flights")]
[Produces("application/json")]
[Authorize]
public class FlightsController : ControllerBase
{
    private readonly IFlightService _flightService;

    public FlightsController(IFlightService flightService)
    {
        _flightService = flightService;
    }

    /// <summary>Searches flights by origin, destination and/or departure date. All filters are
    /// optional and combine with AND; omit all three to list every scheduled flight.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<FlightSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FlightSummaryDto>>> Search(
        [FromQuery] string? originAirportCode,
        [FromQuery] string? destinationAirportCode,
        [FromQuery] DateOnly? departureDate,
        CancellationToken cancellationToken)
    {
        var request = new FlightSearchRequest(originAirportCode, destinationAirportCode, departureDate);
        return Ok(await _flightService.SearchAsync(request, cancellationToken));
    }

    /// <summary>Gets a single flight by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FlightSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FlightSummaryDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _flightService.GetByIdAsync(id, cancellationToken));

    /// <summary>Creates a new scheduled flight. Admin-only: flight scheduling is a back-office
    /// operation, not something a booking agent does mid-call.</summary>
    [HttpPost]
    [Authorize(Roles = nameof(StaffRole.Admin))]
    [ProducesResponseType(typeof(FlightSummaryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FlightSummaryDto>> Create(CreateFlightRequest request, CancellationToken cancellationToken)
    {
        var flight = await _flightService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = flight.Id }, flight);
    }

    /// <summary>Returns the full seat map for a flight with each seat's live availability,
    /// computed by the sp_GetSeatAvailability stored procedure.</summary>
    [HttpGet("{id:int}/seats")]
    [ProducesResponseType(typeof(SeatMapResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeatMapResponse>> GetSeatMap(int id, CancellationToken cancellationToken) =>
        Ok(await _flightService.GetSeatMapAsync(id, cancellationToken));

    /// <summary>Quotes the fare for a flight + fare class combination via the
    /// sp_CalculateFare stored procedure (base fare, multiplier, and a load-factor surcharge
    /// that increases as the flight fills up).</summary>
    [HttpGet("{id:int}/fare-quote")]
    [ProducesResponseType(typeof(FareQuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FareQuoteDto>> GetFareQuote(
        int id, [FromQuery] int fareClassId, CancellationToken cancellationToken) =>
        Ok(await _flightService.GetFareQuoteAsync(id, fareClassId, cancellationToken));
}
