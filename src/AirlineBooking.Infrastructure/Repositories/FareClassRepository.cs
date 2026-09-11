using AirlineBooking.Application.Common.Interfaces;
using AirlineBooking.Domain.Entities;
using AirlineBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AirlineBooking.Infrastructure.Repositories;

public class FareClassRepository : Repository<FareClass>, IFareClassRepository
{
    public FareClassRepository(AirlineBookingDbContext context) : base(context)
    {
    }

    public async Task<FareClass?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(f => f.Code == code, cancellationToken);
}
