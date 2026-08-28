using JazeeraBooking.Application.Features.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JazeeraBooking.Api.Controllers;

/// <summary>Booking lifecycle: hold a seat, confirm it, or cancel it.</summary>
[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>Lists all bookings, most recent first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<BookingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BookingDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _bookingService.GetAllAsync(cancellationToken));

    /// <summary>Gets a booking by its reference code (PNR-style locator, e.g. "K3F7QX").</summary>
    [HttpGet("{reference}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> GetByReference(string reference, CancellationToken cancellationToken) =>
        Ok(await _bookingService.GetByReferenceAsync(reference, cancellationToken));

    /// <summary>Holds a seat for a passenger on a flight at a given fare class. The hold
    /// expires after 15 minutes if not confirmed.</summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/bookings
    ///     {
    ///        "passengerId": 1,
    ///        "flightId": 42,
    ///        "seatId": 517,
    ///        "fareClassId": 3
    ///     }
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> CreateHold(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var booking = await _bookingService.CreateHoldAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByReference), new { reference = booking.BookingReference }, booking);
    }

    /// <summary>Confirms a Held booking. Fails with 409 if the hold has expired or the booking
    /// isn't in Held status.</summary>
    [HttpPost("{reference}/confirm")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> Confirm(string reference, CancellationToken cancellationToken) =>
        Ok(await _bookingService.ConfirmAsync(reference, cancellationToken));

    /// <summary>Cancels a Held or Confirmed booking, freeing the seat.</summary>
    [HttpPost("{reference}/cancel")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> Cancel(string reference, CancellationToken cancellationToken) =>
        Ok(await _bookingService.CancelAsync(reference, cancellationToken));
}
