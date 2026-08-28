using FluentValidation;
using JazeeraBooking.Application.Features.Aircraft;
using JazeeraBooking.Application.Features.Bookings;
using JazeeraBooking.Application.Features.FareClasses;
using JazeeraBooking.Application.Features.Flights;
using JazeeraBooking.Application.Features.Passengers;
using Microsoft.Extensions.DependencyInjection;

namespace JazeeraBooking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAircraftService, AircraftService>();
        services.AddScoped<IFareClassService, FareClassService>();
        services.AddScoped<IFlightService, FlightService>();
        services.AddScoped<IPassengerService, PassengerService>();
        services.AddScoped<IBookingService, BookingService>();

        // Scans this assembly for every AbstractValidator<T> and registers it against
        // IValidator<T>, so new request DTOs get validation for free just by adding a
        // validator class next to them - no per-validator registration line to remember.
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
