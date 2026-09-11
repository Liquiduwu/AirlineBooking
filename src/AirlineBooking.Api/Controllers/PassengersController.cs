using AirlineBooking.Application.Features.Passengers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirlineBooking.Api.Controllers;

/// <summary>Passenger records (travelers). Managed by staff on the passenger's behalf.</summary>
[ApiController]
[Route("api/passengers")]
[Produces("application/json")]
[Authorize]
public class PassengersController : ControllerBase
{
    private readonly IPassengerService _passengerService;

    public PassengersController(IPassengerService passengerService)
    {
        _passengerService = passengerService;
    }

    /// <summary>Lists all passengers.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PassengerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PassengerDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _passengerService.GetAllAsync(cancellationToken));

    /// <summary>Gets a single passenger by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PassengerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PassengerDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _passengerService.GetByIdAsync(id, cancellationToken));

    /// <summary>Creates a new passenger record.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PassengerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PassengerDto>> Create(CreatePassengerRequest request, CancellationToken cancellationToken)
    {
        var passenger = await _passengerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = passenger.Id }, passenger);
    }

    /// <summary>Updates a passenger's contact details.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PassengerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PassengerDto>> Update(int id, UpdatePassengerRequest request, CancellationToken cancellationToken) =>
        Ok(await _passengerService.UpdateAsync(id, request, cancellationToken));
}
