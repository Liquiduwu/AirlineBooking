using JazeeraBooking.Application.Features.Bookings;
using Xunit;

namespace JazeeraBooking.UnitTests.Features.Bookings;

public class BookingReferenceGeneratorTests
{
    [Fact]
    public void Generate_ReturnsSixCharacterCode()
    {
        var reference = BookingReferenceGenerator.Generate();

        Assert.Equal(6, reference.Length);
    }

    [Theory]
    [InlineData('0')]
    [InlineData('O')]
    [InlineData('1')]
    [InlineData('I')]
    public void Generate_NeverContainsAmbiguousCharacters(char ambiguousChar)
    {
        // Ran a large sample rather than asserting on one call, since this is a random
        // generator - a single sample passing wouldn't prove the excluded alphabet.
        for (var i = 0; i < 500; i++)
        {
            var reference = BookingReferenceGenerator.Generate();
            Assert.DoesNotContain(ambiguousChar, reference);
        }
    }
}
