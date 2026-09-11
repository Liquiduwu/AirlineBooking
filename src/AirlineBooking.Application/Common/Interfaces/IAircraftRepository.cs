using AirlineBooking.Domain.Entities;

namespace AirlineBooking.Application.Common.Interfaces;

public interface IAircraftRepository : IRepository<Aircraft>
{
    Task<Aircraft?> GetByRegistrationAsync(string registrationNumber, CancellationToken cancellationToken = default);
}
