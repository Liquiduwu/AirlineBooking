using AirlineBooking.Domain.Enums;

namespace AirlineBooking.Application.Features.FareClasses;

public record FareClassDto(int Id, string Code, string Name, CabinClass CabinClass, decimal PriceMultiplier, bool IsRefundable);

public record CreateFareClassRequest(string Code, string Name, CabinClass CabinClass, decimal PriceMultiplier, bool IsRefundable);
