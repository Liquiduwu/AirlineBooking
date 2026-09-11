using AirlineBooking.Domain.Common;
using AirlineBooking.Domain.Enums;

namespace AirlineBooking.Domain.Entities;

/// <summary>
/// A physical seat on an aircraft (e.g. "12A"). Seats belong to the Aircraft, not the Flight -
/// the same aircraft flies many flights, and the seat map is a property of the airframe. What's
/// available or not is a property of a specific Flight + Seat pairing, so we don't store
/// availability here at all: it's derived from whether an active (Held/Confirmed) Booking
/// exists for that Flight/Seat combination. See BookingConfiguration's filtered unique index
/// for how the database itself enforces "at most one active booking per flight+seat."
/// </summary>
public class Seat : BaseEntity
{
    public int AircraftId { get; set; }
    public Aircraft Aircraft { get; set; } = null!;

    public string SeatNumber { get; set; } = string.Empty;
    public CabinClass CabinClass { get; set; }
    public bool IsExitRow { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
