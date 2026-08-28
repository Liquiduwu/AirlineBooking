using JazeeraBooking.Domain.Entities;

namespace JazeeraBooking.Application.Common.Interfaces;

public interface IStaffUserRepository : IRepository<StaffUser>
{
    Task<StaffUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
