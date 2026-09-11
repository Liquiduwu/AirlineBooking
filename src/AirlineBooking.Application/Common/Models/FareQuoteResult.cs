namespace AirlineBooking.Application.Common.Models;

/// <summary>
/// Shape returned by the sp_CalculateFare stored procedure. Keyless EF Core entity type, same
/// reasoning as <see cref="SeatAvailabilityResult"/>.
/// </summary>
public class FareQuoteResult
{
    public decimal BaseFare { get; set; }
    public decimal PriceMultiplier { get; set; }
    public decimal LoadFactorSurcharge { get; set; }
    public decimal TotalFare { get; set; }
}
