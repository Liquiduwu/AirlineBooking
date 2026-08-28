using JazeeraBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JazeeraBooking.Infrastructure.Persistence.Configurations;

public class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> builder)
    {
        builder.Property(f => f.FlightNumber).HasMaxLength(10).IsRequired();
        builder.Property(f => f.OriginAirportCode).HasMaxLength(3).IsRequired();
        builder.Property(f => f.DestinationAirportCode).HasMaxLength(3).IsRequired();
        builder.Property(f => f.BaseFare).HasPrecision(10, 2);
        builder.Property(f => f.Status).HasConversion<string>().HasMaxLength(20);

        // Every search hits (origin, destination, departure date) - a composite index keeps
        // that the primary path, instead of a full table scan filtered in memory.
        builder.HasIndex(f => new { f.OriginAirportCode, f.DestinationAirportCode, f.DepartureUtc });
    }
}
