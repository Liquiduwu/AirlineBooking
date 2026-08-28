using JazeeraBooking.Application.Features.Aircraft;
using JazeeraBooking.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JazeeraBooking.Api.Controllers;

/// <summary>Aircraft fleet management (reference data used when scheduling flights).</summary>
[ApiController]
[Route("api/aircraft")]
[Produces("application/json")]
[Authorize]
public class AircraftController : ControllerBase
{
    private readonly IAircraftService _aircraftService;

    public AircraftController(IAircraftService aircraftService)
    {
        _aircraftService = aircraftService;
    }

    /// <summary>Lists all aircraft in the fleet.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AircraftDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AircraftDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _aircraftService.GetAllAsync(cancellationToken));

    /// <summary>Gets a single aircraft by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AircraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AircraftDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _aircraftService.GetByIdAsync(id, cancellationToken));

    /// <summary>Registers a new aircraft. Admin-only.</summary>
    [HttpPost]
    [Authorize(Roles = nameof(StaffRole.Admin))]
    [ProducesResponseType(typeof(AircraftDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AircraftDto>> Create(CreateAircraftRequest request, CancellationToken cancellationToken)
    {
        var aircraft = await _aircraftService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = aircraft.Id }, aircraft);
    }
}
