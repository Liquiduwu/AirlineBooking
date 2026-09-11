using AirlineBooking.Application.Common.Interfaces;
using AirlineBooking.Domain.Entities;
using AirlineBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AirlineBooking.Infrastructure.Repositories;

public class StaffUserRepository : Repository<StaffUser>, IStaffUserRepository
{
    public StaffUserRepository(AirlineBookingDbContext context) : base(context)
    {
    }

    public async Task<StaffUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
}
