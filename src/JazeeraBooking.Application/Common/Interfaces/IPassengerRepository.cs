using JazeeraBooking.Domain.Entities;

namespace JazeeraBooking.Application.Common.Interfaces;

public interface IPassengerRepository : IRepository<Passenger>
{
    Task<Passenger?> GetByPassportNumberAsync(string passportNumber, CancellationToken cancellationToken = default);
    Task<Passenger?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
