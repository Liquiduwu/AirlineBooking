using JazeeraBooking.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JazeeraBooking.Infrastructure.Persistence.Configurations;

/// <summary>
/// HasNoKey marks these as query types, not tracked entities: EF Core won't expect a primary
/// key or try to track/persist them, and they can only be materialized via FromSqlRaw/FromSql
/// against the stored procedures they mirror - never via a normal DbSet query against a table.
/// ExcludeFromMigrations matters just as much: without it, EF still assumes a keyless type is
/// backed by a real table/view and will generate a migration to create an empty, never-written
/// "SeatAvailabilityResults" table. These types have no physical backing store at all - the
/// only rows that ever populate them come from EXEC'ing the stored procedure inline.
/// </summary>
public class SeatAvailabilityResultConfiguration : IEntityTypeConfiguration<SeatAvailabilityResult>
{
    public void Configure(EntityTypeBuilder<SeatAvailabilityResult> builder)
    {
        builder.HasNoKey();
        builder.Property(r => r.CabinClass).HasConversion<string>();
        builder.ToTable(t => t.ExcludeFromMigrations());
    }
}

public class FareQuoteResultConfiguration : IEntityTypeConfiguration<FareQuoteResult>
{
    public void Configure(EntityTypeBuilder<FareQuoteResult> builder)
    {
        builder.HasNoKey();
        builder.Property(r => r.BaseFare).HasPrecision(10, 2);
        builder.Property(r => r.PriceMultiplier).HasPrecision(5, 2);
        builder.Property(r => r.LoadFactorSurcharge).HasPrecision(10, 2);
        builder.Property(r => r.TotalFare).HasPrecision(10, 2);
        builder.ToTable(t => t.ExcludeFromMigrations());
    }
}
