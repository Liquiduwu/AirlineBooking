using AirlineBooking.Domain.Entities;

namespace AirlineBooking.Application.Common.Interfaces;

public interface IStaffUserRepository : IRepository<StaffUser>
{
    Task<StaffUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
