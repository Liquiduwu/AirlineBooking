namespace JazeeraBooking.Application.Features.FareClasses;

public interface IFareClassService
{
    Task<List<FareClassDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<FareClassDto> CreateAsync(CreateFareClassRequest request, CancellationToken cancellationToken = default);
}
