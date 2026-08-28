using JazeeraBooking.Application.Common.Interfaces;
using JazeeraBooking.Application.Common.Models;
using JazeeraBooking.Application.Features.Auth;
using JazeeraBooking.Infrastructure.Auth;
using JazeeraBooking.Infrastructure.BackgroundServices;
using JazeeraBooking.Infrastructure.Persistence;
using JazeeraBooking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JazeeraBooking.Infrastructure;

/// <summary>
/// Composition root for this layer: the Api project calls this one extension method instead of
/// reaching into Infrastructure's internals, so Program.cs stays a high-level list of "what
/// this app is made of" rather than a pile of registration details.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("JazeeraBookingDb")
            ?? throw new InvalidOperationException("Connection string 'JazeeraBookingDb' is not configured.");

        services.AddDbContext<JazeeraBookingDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<JazeeraBookingDbContext>());

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
