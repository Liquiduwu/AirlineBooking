using FluentValidation;

namespace JazeeraBooking.Application.Features.Bookings;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.PassengerId).GreaterThan(0);
        RuleFor(x => x.FlightId).GreaterThan(0);
        RuleFor(x => x.SeatId).GreaterThan(0);
        RuleFor(x => x.FareClassId).GreaterThan(0);
    }
}
