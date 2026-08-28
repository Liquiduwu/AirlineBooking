using JazeeraBooking.Domain.Common;

namespace JazeeraBooking.Domain.Entities;

public class Aircraft : BaseEntity
{
    /// <summary>Tail number, e.g. "A6-JZR" - unique per physical airframe.</summary>
    public string RegistrationNumber { get; set; } = string.Empty;

    /// <summary>e.g. "Airbus A320neo". Free text is deliberate for v1 - a normalized
    /// AircraftType lookup table would be the next step if this grew beyond a portfolio scope.</summary>
    public string Model { get; set; } = string.Empty;

    public int TotalSeats { get; set; }

    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
