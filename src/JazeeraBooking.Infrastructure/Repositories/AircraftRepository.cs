using JazeeraBooking.Application.Common.Interfaces;
using JazeeraBooking.Domain.Entities;
using JazeeraBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JazeeraBooking.Infrastructure.Repositories;

public class AircraftRepository : Repository<Aircraft>, IAircraftRepository
{
    public AircraftRepository(JazeeraBookingDbContext context) : base(context)
    {
    }

    public async Task<Aircraft?> GetByRegistrationAsync(string registrationNumber, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(a => a.RegistrationNumber == registrationNumber, cancellationToken);
}
