using AirlineBooking.Application.Common.Interfaces;
using AirlineBooking.Domain.Entities;
using AirlineBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AirlineBooking.Infrastructure.Repositories;

public class AircraftRepository : Repository<Aircraft>, IAircraftRepository
{
    public AircraftRepository(AirlineBookingDbContext context) : base(context)
    {
    }

    public async Task<Aircraft?> GetByRegistrationAsync(string registrationNumber, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(a => a.RegistrationNumber == registrationNumber, cancellationToken);
}
