using AirlineBooking.Application.Common.Interfaces;
using AirlineBooking.Application.Common.Models;
using AirlineBooking.Application.Features.Auth;
using AirlineBooking.Infrastructure.Auth;
using AirlineBooking.Infrastructure.BackgroundServices;
using AirlineBooking.Infrastructure.Persistence;
using AirlineBooking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineBooking.Infrastructure;

/// <summary>
/// Composition root for this layer: the Api project calls this one extension method instead of
/// reaching into Infrastructure's internals, so Program.cs stays a high-level list of "what
/// this app is made of" rather than a pile of registration details.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AirlineBookingDb")
            ?? throw new InvalidOperationException("Connection string 'AirlineBookingDb' is not configured.");

        services.AddDbContext<AirlineBookingDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AirlineBookingDbContext>());

        services.AddScoped<IAircraftRepository, AircraftRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();
        services.AddScoped<IFlightRepository, FlightRepository>();
        services.AddScoped<IFareClassRepository, FareClassRepository>();
        services.AddScoped<IPassengerRepository, PassengerRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IStaffUserRepository, StaffUserRepository>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IAuthService, AuthService>();

        services.AddHostedService<HoldExpirySweepService>();

        return services;
    }
}
