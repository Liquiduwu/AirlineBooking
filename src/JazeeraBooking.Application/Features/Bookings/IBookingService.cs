namespace JazeeraBooking.Application.Features.Bookings;

public interface IBookingService
{
    Task<BookingDto> CreateHoldAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);
    Task<BookingDto> ConfirmAsync(string bookingReference, CancellationToken cancellationToken = default);
    Task<BookingDto> CancelAsync(string bookingReference, CancellationToken cancellationToken = default);
    Task<BookingDto> GetByReferenceAsync(string bookingReference, CancellationToken cancellationToken = default);
    Task<List<BookingDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
