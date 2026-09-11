using System.Text.Json;
using System.Text.Json.Serialization;

namespace AirlineBooking.Api.Serialization;

/// <summary>
/// Every DateTime this API returns (DepartureUtc, HeldUntilUtc, CreatedUtc, ...) is a UTC
/// instant by convention, but EF Core reads SQL Server's Kind-less datetime2 columns back as
/// DateTimeKind.Unspecified. System.Text.Json's default writer serializes an Unspecified value
/// without a "Z"/offset suffix, so a browser parses it as local time instead of UTC - on a
/// client whose timezone isn't UTC, every timestamp (and the booking-hold countdown built on
/// it) comes out shifted by the local offset. Stamping Unspecified as Utc before writing fixes
/// the wire format without touching how EF Core reads the column.
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetDateTime();

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utcValue = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();
        writer.WriteStringValue(utcValue);
    }
}
