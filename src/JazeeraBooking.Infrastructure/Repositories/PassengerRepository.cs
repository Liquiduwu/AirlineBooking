using JazeeraBooking.Application.Common.Interfaces;
using JazeeraBooking.Domain.Entities;
using JazeeraBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JazeeraBooking.Infrastructure.Repositories;

public class PassengerRepository : Repository<Passenger>, IPassengerRepository
{
    public PassengerRepository(JazeeraBookingDbContext context) : base(context)
    {
    }

    public async Task<Passenger?> GetByPassportNumberAsync(string passportNumber, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(p => p.PassportNumber == passportNumber, cancellationToken);

    public async Task<Passenger?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
}
