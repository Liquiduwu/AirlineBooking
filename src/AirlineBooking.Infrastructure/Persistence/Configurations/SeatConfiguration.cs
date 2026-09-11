using AirlineBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineBooking.Infrastructure.Persistence.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.Property(s => s.SeatNumber).HasMaxLength(4).IsRequired();
        builder.Property(s => s.CabinClass).HasConversion<string>().HasMaxLength(20);

        // A given aircraft can't have the same seat number twice.
        builder.HasIndex(s => new { s.AircraftId, s.SeatNumber }).IsUnique();
    }
}
