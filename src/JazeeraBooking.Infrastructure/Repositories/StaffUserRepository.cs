using JazeeraBooking.Application.Common.Interfaces;
using JazeeraBooking.Domain.Entities;
using JazeeraBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JazeeraBooking.Infrastructure.Repositories;

public class StaffUserRepository : Repository<StaffUser>, IStaffUserRepository
{
    public StaffUserRepository(JazeeraBookingDbContext context) : base(context)
    {
    }

    public async Task<StaffUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
}
