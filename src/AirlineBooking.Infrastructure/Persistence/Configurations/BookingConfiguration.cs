using AirlineBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineBooking.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.Property(b => b.BookingReference).HasMaxLength(6).IsRequired();
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.FareAmount).HasPrecision(10, 2);

        builder.HasIndex(b => b.BookingReference).IsUnique();

        // The real invariant this API depends on: a seat can have at most one *active*
        // booking per flight. Enforcing this only in the service layer leaves a race window
        // between two concurrent hold requests for the same seat both passing the app-level
        // check before either commits. A SQL Server filtered unique index closes that window
        // at the database level regardless of app-layer concurrency bugs - defense in depth,
        // not a replacement for the app-level check (which still gives a friendly 409 instead
        // of a raw constraint-violation exception in the common case).
        builder.HasIndex(b => new { b.FlightId, b.SeatId })
            .IsUnique()
            .HasFilter("[Status] IN ('Held','Confirmed')")
            .HasDatabaseName("IX_Booking_ActiveSeatPerFlight");

        builder.HasOne(b => b.Passenger)
            .WithMany(p => p.Bookings)
            .HasForeignKey(b => b.PassengerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Flight)
            .WithMany(f => f.Bookings)
            .HasForeignKey(b => b.FlightId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Seat)
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.FareClass)
            .WithMany(f => f.Bookings)
            .HasForeignKey(b => b.FareClassId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
