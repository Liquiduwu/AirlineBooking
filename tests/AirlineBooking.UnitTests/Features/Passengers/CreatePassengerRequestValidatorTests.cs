using AirlineBooking.Application.Features.Passengers;
using FluentValidation.TestHelper;
using Xunit;

namespace AirlineBooking.UnitTests.Features.Passengers;

public class CreatePassengerRequestValidatorTests
{
    private readonly CreatePassengerRequestValidator _validator = new();

    private static CreatePassengerRequest ValidRequest() => new(
        "Yousef", "Ali", "yousef@example.com", "+96550001234", "P1234567", new DateOnly(1995, 5, 20));

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyFirstName_HasErrorForFirstName()
    {
        var request = ValidRequest() with { FirstName = "" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Validate_InvalidEmail_HasErrorForEmail()
    {
        var request = ValidRequest() with { Email = "not-an-email" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_FutureDateOfBirth_HasErrorForDateOfBirth()
    {
        var request = ValidRequest() with { DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }
}
