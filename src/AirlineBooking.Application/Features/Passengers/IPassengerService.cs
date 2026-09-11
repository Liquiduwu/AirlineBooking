namespace AirlineBooking.Application.Features.Passengers;

public interface IPassengerService
{
    Task<List<PassengerDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PassengerDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PassengerDto> CreateAsync(CreatePassengerRequest request, CancellationToken cancellationToken = default);
    Task<PassengerDto> UpdateAsync(int id, UpdatePassengerRequest request, CancellationToken cancellationToken = default);
}
