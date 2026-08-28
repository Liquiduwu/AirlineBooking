namespace JazeeraBooking.Domain.Enums;

/// <summary>
/// A booking starts as Held (seat reserved, payment/confirmation pending - mirrors how real
/// GDS/airline systems avoid double-selling a seat while an agent finishes a call).
/// If it isn't confirmed before <see cref="Domain.Entities.Booking.HeldUntilUtc"/>, a
/// background sweep flips it to Expired, freeing the seat. Cancelled is an explicit,
/// agent/passenger-initiated terminal state, kept distinct from Expired so reporting can
/// tell "changed their mind" apart from "never followed through."
/// </summary>
public enum BookingStatus
{
    Held = 0,
    Confirmed = 1,
    Cancelled = 2,
    Expired = 3
}
