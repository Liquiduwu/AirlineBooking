using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using JazeeraBooking.Application.Features.Auth;
using JazeeraBooking.Application.Features.Bookings;
using JazeeraBooking.Application.Features.Flights;
using JazeeraBooking.Application.Features.Passengers;
using JazeeraBooking.Domain.Enums;
using JazeeraBooking.Infrastructure.Persistence;
using Xunit;

namespace JazeeraBooking.IntegrationTests;

/// <summary>
/// Exercises the full hold -> confirm booking flow, and the seat-conflict path, against a real
/// SQL Server (see CustomWebApplicationFactory) - this is specifically what a unit test with
/// mocked repositories cannot prove: that the stored procedures return the right shape, that
/// the filtered unique index actually rejects a concurrent double-booking, and that the whole
/// request pipeline (auth, validation, controller, service, EF Core, SQL Server) works
/// together end to end.
/// </summary>
[Collection("Integration")]
public class BookingLifecycleTests
{
    private readonly CustomWebApplicationFactory _factory;

    public BookingLifecycleTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest(DbSeeder.SeedAdminUsername, DbSeeder.SeedAdminPassword));
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions.Default);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        return client;
    }

    [Fact]
    public async Task HoldThenConfirm_OnAvailableSeat_Succeeds()
    {
        var client = await CreateAuthenticatedClientAsync();

        var flights = await (await client.GetAsync("/api/flights")).Content.ReadFromJsonAsync<List<FlightSummaryDto>>(JsonOptions.Default);
        var flight = Assert.Single(flights!);

        var seatMap = await (await client.GetAsync($"/api/flights/{flight.Id}/seats")).Content
            .ReadFromJsonAsync<SeatMapResponse>(JsonOptions.Default);
        var seat = Assert.Single(seatMap!.Seats, s => s.SeatNumber == "1A");
        Assert.True(seat.IsAvailable);

        var passengerResponse = await client.PostAsJsonAsync(
            "/api/passengers",
            new CreatePassengerRequest("Integration", "Tester", "integration.tester@example.com", null, "P9999999", new DateOnly(1990, 1, 1)));
        Assert.Equal(HttpStatusCode.Created, passengerResponse.StatusCode);
        var passenger = await passengerResponse.Content.ReadFromJsonAsync<PassengerDto>(JsonOptions.Default);

        var fareClasses = await (await client.GetAsync("/api/fare-classes")).Content
            .ReadFromJsonAsync<List<Application.Features.FareClasses.FareClassDto>>(JsonOptions.Default);
        var businessFareClass = Assert.Single(fareClasses!, f => f.Code == "J");

        var holdResponse = await client.PostAsJsonAsync(
            "/api/bookings",
            new CreateBookingRequest(passenger!.Id, flight.Id, seat.SeatId, businessFareClass.Id));
        Assert.Equal(HttpStatusCode.Created, holdResponse.StatusCode);
        var held = await holdResponse.Content.ReadFromJsonAsync<BookingDto>(JsonOptions.Default);
        Assert.Equal(BookingStatus.Held, held!.Status);

        var confirmResponse = await client.PostAsync($"/api/bookings/{held.BookingReference}/confirm", content: null);
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
        var confirmed = await confirmResponse.Content.ReadFromJsonAsync<BookingDto>(JsonOptions.Default);
        Assert.Equal(BookingStatus.Confirmed, confirmed!.Status);

        var seatMapAfter = await (await client.GetAsync($"/api/flights/{flight.Id}/seats")).Content
            .ReadFromJsonAsync<SeatMapResponse>(JsonOptions.Default);
        var seatAfter = Assert.Single(seatMapAfter!.Seats, s => s.SeatNumber == "1A");
        Assert.False(seatAfter.IsAvailable);
    }

    [Fact]
    public async Task Hold_OnSeatAlreadyHeld_ReturnsConflict()
    {
        var client = await CreateAuthenticatedClientAsync();

        var flights = await (await client.GetAsync("/api/flights")).Content.ReadFromJsonAsync<List<FlightSummaryDto>>(JsonOptions.Default);
        var flight = Assert.Single(flights!);

        var seatMap = await (await client.GetAsync($"/api/flights/{flight.Id}/seats")).Content
            .ReadFromJsonAsync<SeatMapResponse>(JsonOptions.Default);
        var seat = Assert.Single(seatMap!.Seats, s => s.SeatNumber == "2A");

        var fareClasses = await (await client.GetAsync("/api/fare-classes")).Content
            .ReadFromJsonAsync<List<Application.Features.FareClasses.FareClassDto>>(JsonOptions.Default);
        var fareClass = Assert.Single(fareClasses!, f => f.Code == "J");

        var firstPassenger = await (await client.PostAsJsonAsync(
                "/api/passengers",
                new CreatePassengerRequest("First", "Passenger", "first.passenger@example.com", null, "P1111111", new DateOnly(1990, 1, 1))))
            .Content.ReadFromJsonAsync<PassengerDto>(JsonOptions.Default);
        var secondPassenger = await (await client.PostAsJsonAsync(
                "/api/passengers",
                new CreatePassengerRequest("Second", "Passenger", "second.passenger@example.com", null, "P2222222", new DateOnly(1990, 1, 1))))
            .Content.ReadFromJsonAsync<PassengerDto>(JsonOptions.Default);

        var firstHold = await client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(firstPassenger!.Id, flight.Id, seat.SeatId, fareClass.Id));
        Assert.Equal(HttpStatusCode.Created, firstHold.StatusCode);

        var secondHold = await client.PostAsJsonAsync(
            "/api/bookings", new CreateBookingRequest(secondPassenger!.Id, flight.Id, seat.SeatId, fareClass.Id));
        Assert.Equal(HttpStatusCode.Conflict, secondHold.StatusCode);
    }
}
