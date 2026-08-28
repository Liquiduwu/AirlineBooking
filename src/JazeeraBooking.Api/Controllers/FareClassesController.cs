using JazeeraBooking.Application.Features.FareClasses;
using JazeeraBooking.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JazeeraBooking.Api.Controllers;

/// <summary>Fare class / pricing-rule reference data (e.g. "Y" economy saver, "J" business).</summary>
[ApiController]
[Route("api/fare-classes")]
[Produces("application/json")]
[Authorize]
public class FareClassesController : ControllerBase
{
    private readonly IFareClassService _fareClassService;

    public FareClassesController(IFareClassService fareClassService)
    {
        _fareClassService = fareClassService;
    }

    /// <summary>Lists all fare classes.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<FareClassDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FareClassDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _fareClassService.GetAllAsync(cancellationToken));

    /// <summary>Creates a new fare class. Admin-only.</summary>
    [HttpPost]
    [Authorize(Roles = nameof(StaffRole.Admin))]
    [ProducesResponseType(typeof(FareClassDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FareClassDto>> Create(CreateFareClassRequest request, CancellationToken cancellationToken)
    {
        var fareClass = await _fareClassService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), fareClass);
    }
}
