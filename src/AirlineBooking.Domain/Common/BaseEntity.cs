namespace AirlineBooking.Domain.Common;

/// <summary>
/// Every entity gets an int identity key plus audit timestamps. int (not Guid) keys are
/// deliberate: they're smaller, sequential (so SQL Server clustered-index inserts don't
/// fragment the way random Guids do), and human-friendly in logs/debugging. Where a value
/// needs to be safe to expose publicly (e.g. a booking locator), we add a separate string
/// field rather than exposing the identity column itself.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; set; }
}
