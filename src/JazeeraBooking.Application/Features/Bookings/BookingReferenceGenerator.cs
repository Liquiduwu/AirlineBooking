namespace JazeeraBooking.Application.Features.Bookings;

/// <summary>
/// Generates 6-character locator codes in the style of a real airline PNR. The alphabet
/// excludes 0/O and 1/I - characters that are easy to misread over the phone or mistype,
/// which matters because agents read these out loud to passengers. Not a security boundary
/// (this API has no unauthenticated "look up my booking" endpoint - see README) so
/// System.Random is fine here; nothing relies on the code being unguessable.
/// </summary>
public static class BookingReferenceGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate() =>
        new(Enumerable.Range(0, 6).Select(_ => Alphabet[Random.Shared.Next(Alphabet.Length)]).ToArray());
}
