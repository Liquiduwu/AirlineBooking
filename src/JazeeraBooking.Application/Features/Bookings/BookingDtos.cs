using JazeeraBooking.Domain.Enums;

namespace JazeeraBooking.Application.Features.Bookings;

public record BookingDto(
    int Id,
    string BookingReference,
    int PassengerId,
    string PassengerName,
    int FlightId,
    string FlightNumber,
    int SeatId,
    string SeatNumber,
    int FareClassId,
    string FareClassCode,
    BookingStatus Status,
    decimal FareAmount,
    DateTime? HeldUntilUtc,
    DateTime? ConfirmedUtc,
    DateTime? CancelledUtc,
    DateTime CreatedUtc);

public record CreateBookingRequest(int PassengerId, int FlightId, int SeatId, int FareClassId);
