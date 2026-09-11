using AirlineBooking.Domain.Entities;

namespace AirlineBooking.Application.Common.Interfaces;

public interface IFareClassRepository : IRepository<FareClass>
{
    Task<FareClass?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
