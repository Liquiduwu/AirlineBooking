namespace AirlineBooking.Application.Common.Interfaces;

/// <summary>
/// Wraps DbContext.SaveChangesAsync so services commit explicitly after one or more repository
/// calls, instead of every repository method saving independently. This matters for operations
/// like "create booking" that touch multiple entities (Booking insert + Seat/Flight reads) and
/// need to succeed or fail together as one transaction.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
