using System.Text.Json;
using System.Text.Json.Serialization;

namespace JazeeraBooking.IntegrationTests;

/// <summary>
/// The API serializes enums as strings and camelCases property names (ASP.NET Core MVC's
/// controller default) - HttpContent.ReadFromJsonAsync&lt;T&gt; needs matching options on the
/// reading side, since it doesn't share the server's JsonOptions by default.
/// JsonSerializerDefaults.Web brings camelCase naming + case-insensitive property matching;
/// without it, "accessToken" in the response body silently fails to bind to the AccessToken
/// record parameter and comes back null instead of throwing.
/// </summary>
public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
