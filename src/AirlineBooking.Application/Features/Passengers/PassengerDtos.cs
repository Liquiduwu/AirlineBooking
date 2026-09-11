namespace AirlineBooking.Application.Features.Passengers;

public record PassengerDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string PassportNumber,
    DateOnly DateOfBirth);

public record CreatePassengerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string PassportNumber,
    DateOnly DateOfBirth);

public record UpdatePassengerRequest(string FirstName, string LastName, string Email, string? PhoneNumber);
