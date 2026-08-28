using JazeeraBooking.Domain.Entities;

namespace JazeeraBooking.Application.Common.Interfaces;

public interface IAircraftRepository : IRepository<Aircraft>
{
    Task<Aircraft?> GetByRegistrationAsync(string registrationNumber, CancellationToken cancellationToken = default);
}
