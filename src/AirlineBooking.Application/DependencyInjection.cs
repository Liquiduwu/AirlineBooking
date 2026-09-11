using FluentValidation;
using AirlineBooking.Application.Features.Aircraft;
using AirlineBooking.Application.Features.Bookings;
using AirlineBooking.Application.Features.FareClasses;
using AirlineBooking.Application.Features.Flights;
using AirlineBooking.Application.Features.Passengers;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineBooking.Application;

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
