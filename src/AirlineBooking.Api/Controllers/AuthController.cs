using AirlineBooking.Application.Features.Auth;
using AirlineBooking.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AirlineBooking.Api.Controllers;

/// <summary>Staff authentication - issues and manages JWTs for booking agents and admins.</summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Authenticates a staff user and issues a JWT access token.</summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/auth/login
    ///     {
    ///        "username": "agent01",
    ///        "password": "P@ssw0rd123"
    ///     }
    ///
    /// Use the returned <c>accessToken</c> as a Bearer token on subsequent requests.
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>Registers a new staff account. Admin-only: this is how the first Agent gets
    /// onboarded by an Admin, not a public sign-up endpoint.</summary>
    [HttpPost("register")]
    [Authorize(Roles = nameof(StaffRole.Admin))]
    [ProducesResponseType(typeof(StaffUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StaffUserDto>> Register(RegisterStaffRequest request, CancellationToken cancellationToken)
    {
        var user = await _authService.RegisterStaffAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
    }
}
