using AirlineBooking.Domain.Entities;
using AirlineBooking.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AirlineBooking.Infrastructure.Persistence;

/// <summary>
/// Development-only convenience: registering staff is Admin-only (see AuthController.Register),
/// which is a chicken-and-egg problem for a freshly migrated, empty database - there's no
/// Admin yet to create the first Admin. This seeds one, plus enough reference data (an
/// aircraft, a couple of fare classes, a sample flight) to exercise every endpoint without
/// hand-crafting rows first. Never runs outside Development (wired up that way in Program.cs).
/// </summary>
public static class DbSeeder
{
    public const string SeedAdminUsername = "admin";
    public const string SeedAdminPassword = "Admin#Passw0rd1";

    public static async Task SeedAsync(AirlineBookingDbContext context)
    {
        if (!await context.StaffUsers.AnyAsync())
        {
            var hasher = new PasswordHasher<StaffUser>();
            var admin = new StaffUser
            {
                Username = SeedAdminUsername,
                Email = "admin@airline-booking.local",
                Role = StaffRole.Admin,
                IsActive = true
            };
            admin.PasswordHash = hasher.HashPassword(admin, SeedAdminPassword);
            context.StaffUsers.Add(admin);
        }

        if (!await context.Aircraft.AnyAsync())
        {
            var aircraft = new Aircraft
            {
                RegistrationNumber = "A6-JZR",
                Model = "Airbus A320neo",
                TotalSeats = 12
            };

            for (var row = 1; row <= 2; row++)
            {
                foreach (var letter in new[] { 'A', 'B', 'C' })
                {
                    aircraft.Seats.Add(new Seat { SeatNumber = $"{row}{letter}", CabinClass = CabinClass.Business, IsExitRow = row == 1 });
                }
            }

            for (var row = 3; row <= 4; row++)
            {
                foreach (var letter in new[] { 'A', 'B', 'C' })
                {
                    aircraft.Seats.Add(new Seat { SeatNumber = $"{row}{letter}", CabinClass = CabinClass.Economy, IsExitRow = false });
                }
            }

            context.Aircraft.Add(aircraft);
            await context.SaveChangesAsync();

            var departure = DateTime.UtcNow.Date.AddDays(3).AddHours(9);
            context.Flights.Add(new Flight
            {
                FlightNumber = "J9 101",
                OriginAirportCode = "KWI",
                DestinationAirportCode = "DXB",
                DepartureUtc = departure,
                ArrivalUtc = departure.AddHours(1).AddMinutes(20),
                AircraftId = aircraft.Id,
                BaseFare = 85.00m,
                Status = FlightStatus.Scheduled
            });
        }

        if (!await context.FareClasses.AnyAsync())
        {
            context.FareClasses.AddRange(
                new FareClass { Code = "Y", Name = "Economy Saver", CabinClass = CabinClass.Economy, PriceMultiplier = 1.0m, IsRefundable = false },
                new FareClass { Code = "K", Name = "Economy Flex", CabinClass = CabinClass.Economy, PriceMultiplier = 1.35m, IsRefundable = true },
                new FareClass { Code = "J", Name = "Business", CabinClass = CabinClass.Business, PriceMultiplier = 2.5m, IsRefundable = true });
        }

        await context.SaveChangesAsync();
    }
}
