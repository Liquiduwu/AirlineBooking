using AirlineBooking.Domain.Common;
using AirlineBooking.Domain.Enums;

namespace AirlineBooking.Domain.Entities;

public class Flight : BaseEntity
{
    /// <summary>e.g. "J9 101".</summary>
    public string FlightNumber { get; set; } = string.Empty;

    /// <summary>IATA 3-letter airport code, e.g. "KWI".</summary>
    public string OriginAirportCode { get; set; } = string.Empty;

    /// <summary>IATA 3-letter airport code, e.g. "DXB".</summary>
    public string DestinationAirportCode { get; set; } = string.Empty;

    public DateTime DepartureUtc { get; set; }
    public DateTime ArrivalUtc { get; set; }

    public int AircraftId { get; set; }
    public Aircraft Aircraft { get; set; } = null!;

    /// <summary>Base price before a <see cref="FareClass.PriceMultiplier"/> is applied.</summary>
    public decimal BaseFare { get; set; }

    public FlightStatus Status { get; set; } = FlightStatus.Scheduled;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
