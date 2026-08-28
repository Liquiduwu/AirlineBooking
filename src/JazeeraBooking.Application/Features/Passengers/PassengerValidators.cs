using FluentValidation;

namespace JazeeraBooking.Application.Features.Passengers;

public class CreatePassengerRequestValidator : AbstractValidator<CreatePassengerRequest>
{
    public CreatePassengerRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
        RuleFor(x => x.PassportNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(x => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth must be in the past.");
    }
}

public class UpdatePassengerRequestValidator : AbstractValidator<UpdatePassengerRequest>
{
    public UpdatePassengerRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
    }
}
