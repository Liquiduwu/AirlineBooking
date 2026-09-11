using AirlineBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineBooking.Infrastructure.Persistence.Configurations;

public class FareClassConfiguration : IEntityTypeConfiguration<FareClass>
{
    public void Configure(EntityTypeBuilder<FareClass> builder)
    {
        builder.Property(f => f.Code).HasMaxLength(2).IsRequired();
        builder.Property(f => f.Name).HasMaxLength(50).IsRequired();
        builder.Property(f => f.CabinClass).HasConversion<string>().HasMaxLength(20);
        builder.Property(f => f.PriceMultiplier).HasPrecision(5, 2);

        builder.HasIndex(f => f.Code).IsUnique();
    }
}
