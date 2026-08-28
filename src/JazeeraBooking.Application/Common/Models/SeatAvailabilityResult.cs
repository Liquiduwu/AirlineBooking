using JazeeraBooking.Domain.Enums;

namespace JazeeraBooking.Application.Common.Models;

/// <summary>
/// Shape returned by the sp_GetSeatAvailability stored procedure. This is a keyless EF Core
/// entity type (no Id, not tracked) - it exists purely to give FromSqlRaw a typed row to
/// materialize into, not to be persisted.
/// </summary>
public class SeatAvailabilityResult
{
    public int SeatId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public CabinClass CabinClass { get; set; }
    public bool IsExitRow { get; set; }
    public bool IsAvailable { get; set; }
}
