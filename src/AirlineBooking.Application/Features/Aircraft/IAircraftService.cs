namespace AirlineBooking.Application.Features.Aircraft;

public interface IAircraftService
{
    Task<List<AircraftDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AircraftDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AircraftDto> CreateAsync(CreateAircraftRequest request, CancellationToken cancellationToken = default);
}
