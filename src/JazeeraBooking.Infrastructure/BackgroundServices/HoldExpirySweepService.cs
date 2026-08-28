using JazeeraBooking.Application.Common.Interfaces;
using JazeeraBooking.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JazeeraBooking.Infrastructure.BackgroundServices;

/// <summary>
/// BackgroundService is a singleton (registered once, lives for the app's lifetime), but
/// DbContext/repositories are scoped (one instance per request) - mixing those lifetimes
/// directly would either throw or leak a DbContext across requests. IServiceScopeFactory is
/// the standard fix: create a fresh DI scope for each sweep tick, resolve scoped services
/// inside it, and let the scope (and its DbContext) get disposed at the end of the tick.
/// Booking.ConfirmAsync also does a lazy per-request expiry check as a belt-and-braces
/// backstop, so a booking never reads as "Held" past its HeldUntilUtc even if this sweep is
/// running slowly - the invariant doesn't depend on this timer's exact cadence.
/// </summary>
public class HoldExpirySweepService : BackgroundService
{
    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HoldExpirySweepService> _logger;

    public HoldExpirySweepService(IServiceScopeFactory scopeFactory, ILogger<HoldExpirySweepService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(SweepInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await SweepExpiredHoldsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Hold-expiry sweep failed; will retry on the next tick.");
            }
        }
    }

    private async Task SweepExpiredHoldsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var expired = await bookingRepository.GetExpiredHoldsAsync(DateTime.UtcNow, cancellationToken);
        if (expired.Count == 0)
        {
            return;
        }

        foreach (var booking in expired)
        {
            booking.Status = BookingStatus.Expired;
            booking.HeldUntilUtc = null;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Expired {Count} booking hold(s) that passed their HeldUntilUtc.", expired.Count);
    }
}
