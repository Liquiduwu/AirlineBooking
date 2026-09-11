using AirlineBooking.Application.Common.Exceptions;
using AirlineBooking.Application.Common.Interfaces;
using AirlineBooking.Application.Common.Models;
using AirlineBooking.Domain.Common;
using AirlineBooking.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AirlineBooking.Infrastructure.Persistence;

/// <summary>
/// The EF Core DbContext doubles as our IUnitOfWork implementation - it already tracks every
/// change made through its DbSets and commits them atomically in one transaction on
/// SaveChangesAsync, which is exactly what a Unit of Work is. Rather than write a second
/// wrapper class around it, we implement the interface directly on the context.
/// </summary>
public class AirlineBookingDbContext : DbContext, IUnitOfWork
{
    public AirlineBookingDbContext(DbContextOptions<AirlineBookingDbContext> options) : base(options)
    {
    }

    public DbSet<Aircraft> Aircraft => Set<Aircraft>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<FareClass> FareClasses => Set<FareClass>();
    public DbSet<Passenger> Passengers => Set<Passenger>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<StaffUser> StaffUsers => Set<StaffUser>();

    // Keyless types used only as materialization targets for stored-procedure result sets.
    public DbSet<SeatAvailabilityResult> SeatAvailabilityResults => Set<SeatAvailabilityResult>();
    public DbSet<FareQuoteResult> FareQuoteResults => Set<FareQuoteResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AirlineBookingDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedUtc = utcNow;
            }
        }

        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Translate a persistence-specific failure into an Application-level exception at
            // this boundary, so services (which must stay EF-Core-agnostic) can catch
            // ConflictException without knowing SQL Server error numbers exist. This is what
            // actually closes the seat-double-booking race: two concurrent holds can both pass
            // the app-level "is this seat taken" check, but only one insert satisfies the
            // filtered unique index in BookingConfiguration - the loser lands here.
            throw new ConflictException(
                "The request conflicts with existing data (e.g. a seat was just taken by another booking, or a duplicate value was used).");
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}
