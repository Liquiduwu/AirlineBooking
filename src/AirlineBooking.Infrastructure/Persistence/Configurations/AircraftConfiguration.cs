using AirlineBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineBooking.Infrastructure.Persistence.Configurations;

public class AircraftConfiguration : IEntityTypeConfiguration<Aircraft>
{
    public void Configure(EntityTypeBuilder<Aircraft> builder)
    {
        builder.Property(a => a.RegistrationNumber).HasMaxLength(10).IsRequired();
        builder.Property(a => a.Model).HasMaxLength(100).IsRequired();

        builder.HasIndex(a => a.RegistrationNumber).IsUnique();

        builder.HasMany(a => a.Seats)
            .WithOne(s => s.Aircraft)
            .HasForeignKey(s => s.AircraftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Flights)
            .WithOne(f => f.Aircraft)
            .HasForeignKey(f => f.AircraftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
