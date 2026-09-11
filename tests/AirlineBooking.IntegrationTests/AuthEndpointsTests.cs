using System.Net;
using System.Net.Http.Json;
using AirlineBooking.Application.Features.Auth;
using AirlineBooking.Infrastructure.Persistence;
using Xunit;

namespace AirlineBooking.IntegrationTests;

[Collection("Integration")]
public class AuthEndpointsTests
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithSeededAdminCredentials_ReturnsTokenAndAdminRole()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(DbSeeder.SeedAdminUsername, DbSeeder.SeedAdminPassword));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions.Default);
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.AccessToken));
        Assert.Equal(Domain.Enums.StaffRole.Admin, body.Role);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(DbSeeder.SeedAdminUsername, "definitely-wrong"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/flights");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
