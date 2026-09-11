using AirlineBooking.Application.Common.Interfaces;
using AirlineBooking.Domain.Entities;
using AirlineBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AirlineBooking.Infrastructure.Repositories;

public class PassengerRepository : Repository<Passenger>, IPassengerRepository
{
    public PassengerRepository(AirlineBookingDbContext context) : base(context)
    {
    }

    public async Task<Passenger?> GetByPassportNumberAsync(string passportNumber, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(p => p.PassportNumber == passportNumber, cancellationToken);

    public async Task<Passenger?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
}
