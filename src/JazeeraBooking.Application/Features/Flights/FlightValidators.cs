using FluentValidation;

namespace JazeeraBooking.Application.Features.Flights;

public class CreateFlightRequestValidator : AbstractValidator<CreateFlightRequest>
{
    public CreateFlightRequestValidator()
    {
        RuleFor(x => x.FlightNumber).NotEmpty().MaximumLength(10);
        RuleFor(x => x.OriginAirportCode).NotEmpty().Length(3);
        RuleFor(x => x.DestinationAirportCode).NotEmpty().Length(3)
            .NotEqual(x => x.OriginAirportCode).WithMessage("Destination must differ from origin.");
        RuleFor(x => x.DepartureUtc).NotEmpty();
        RuleFor(x => x.ArrivalUtc).GreaterThan(x => x.DepartureUtc).WithMessage("Arrival must be after departure.");
        RuleFor(x => x.AircraftId).GreaterThan(0);
        RuleFor(x => x.BaseFare).GreaterThan(0);
    }
}
