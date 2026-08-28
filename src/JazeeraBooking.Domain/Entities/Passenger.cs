using JazeeraBooking.Domain.Common;

namespace JazeeraBooking.Domain.Entities;

/// <summary>
/// A traveler. Not an authenticated actor in this system - staff (see
/// <see cref="StaffUser"/>) create and manage bookings on passengers' behalf, which matches
/// how airline reservation/GDS systems actually work (an agent, not the traveler, drives the
/// booking tool). A self-service passenger portal is listed as future work in the README.
/// </summary>
public class Passenger : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string PassportNumber { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
