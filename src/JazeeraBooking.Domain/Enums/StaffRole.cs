namespace JazeeraBooking.Domain.Enums;

/// <summary>
/// Roles for internal staff users (this is a B2B/internal-tools API, not a passenger-facing
/// consumer app - passengers are data (see <see cref="Domain.Entities.Passenger"/>), not
/// authenticated actors). Agent can search/hold/confirm bookings; Admin additionally manages
/// flights, aircraft and fare classes.
/// </summary>
public enum StaffRole
{
    Agent = 0,
    Admin = 1
}
