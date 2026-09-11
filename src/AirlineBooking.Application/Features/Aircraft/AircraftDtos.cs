namespace AirlineBooking.Application.Features.Aircraft;

public record AircraftDto(int Id, string RegistrationNumber, string Model, int TotalSeats);

public record CreateAircraftRequest(string RegistrationNumber, string Model, int TotalSeats);
