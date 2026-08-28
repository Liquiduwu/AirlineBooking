using JazeeraBooking.Domain.Entities;

namespace JazeeraBooking.Application.Common.Interfaces;

public interface IFareClassRepository : IRepository<FareClass>
{
    Task<FareClass?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
