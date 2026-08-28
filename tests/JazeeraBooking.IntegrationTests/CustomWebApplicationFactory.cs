using JazeeraBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace JazeeraBooking.IntegrationTests;

/// <summary>
/// Spins up a real SQL Server in a throwaway Docker container per test run (via Testcontainers)
/// rather than mocking the database or using EF Core's InMemory provider. InMemory doesn't
/// enforce the constraints this app actually depends on for correctness - the filtered unique
/// index that prevents double-booking a seat (see BookingConfiguration) and the raw stored
/// procedures (see sp_GetSeatAvailability / sp_CalculateFare) simply don't exist outside real
/// SQL Server, so testing against InMemory would give false confidence on exactly the parts
/// most worth testing. The container starts fresh, gets migrated, and is torn down after the
/// test run - no shared state between runs, no dependency on a pre-existing local database.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // UseSetting (not ConfigureAppConfiguration) is the reliable way to override config for
        // a minimal-hosting Program.cs under test: settings passed here seed the
        // WebApplicationBuilder before Program.cs's top-level statements run, so code that
        // reads builder.Configuration directly (like AddInfrastructure's connection-string
        // lookup, which executes before builder.Build()) sees the override. Config added via
        // ConfigureAppConfiguration only reliably reaches DI-resolved IConfiguration *after*
        // Build() - too late for that call.
        builder.UseSetting("ConnectionStrings:JazeeraBookingDb", _sqlContainer.GetConnectionString());
        builder.UseSetting("Jwt:Issuer", "JazeeraBooking.Tests");
        builder.UseSetting("Jwt:Audience", "JazeeraBooking.Tests.Client");
        builder.UseSetting("Jwt:SigningKey", "integration-test-signing-key-at-least-32-chars-long");
        builder.UseSetting("Jwt:AccessTokenMinutes", "60");
    }

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        // Force host creation now (rather than lazily on first HttpClient use) so migrations
        // and seeding are guaranteed to finish before any test issues a request.
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<JazeeraBookingDbContext>();
        await dbContext.Database.MigrateAsync();
        await DbSeeder.SeedAsync(dbContext);
    }

    public new async Task DisposeAsync()
    {
        await _sqlContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
