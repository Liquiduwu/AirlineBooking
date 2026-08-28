using JazeeraBooking.Application.Common.Interfaces;
using JazeeraBooking.Domain.Entities;
using JazeeraBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JazeeraBooking.Infrastructure.Repositories;

public class FareClassRepository : Repository<FareClass>, IFareClassRepository
{
    public FareClassRepository(JazeeraBookingDbContext context) : base(context)
    {
    }

    public async Task<FareClass?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(f => f.Code == code, cancellationToken);
}
