using JazeeraBooking.Domain.Common;
using JazeeraBooking.Domain.Enums;

namespace JazeeraBooking.Domain.Entities;

/// <summary>
/// A fare bucket (real airlines call these booking classes: Y, B, M for economy; J, C for
/// business). Each carries a multiplier applied to the flight's <see cref="Flight.BaseFare"/>,
/// so the same route can be sold at different price points (flexible vs. saver) without
/// duplicating flight rows. Kept as a global lookup table rather than per-flight rows because
/// the pricing *rule* (multiplier, refundability) is a product of the fare class, not the
/// individual flight - the flight only supplies the base fare the multiplier is applied to.
/// </summary>
public class FareClass : BaseEntity
{
    /// <summary>Short code, e.g. "Y", "K", "J" - what agents/GDS systems use for shorthand.</summary>
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public CabinClass CabinClass { get; set; }
    public decimal PriceMultiplier { get; set; }
    public bool IsRefundable { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
