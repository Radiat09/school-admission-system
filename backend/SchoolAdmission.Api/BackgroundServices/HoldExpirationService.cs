using Microsoft.Extensions.Logging;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Services;

namespace SchoolAdmission.Api.BackgroundServices;

public class HoldExpirationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HoldExpirationService> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly int _holdDurationSeconds;

    public HoldExpirationService(
        IServiceScopeFactory scopeFactory,
        ILogger<HoldExpirationService> logger,
        TimeProvider timeProvider,
        int holdDurationSeconds)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _timeProvider = timeProvider;
        _holdDurationSeconds = holdDurationSeconds;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Hold expiration service started. Hold duration: {DurationSeconds}s", _holdDurationSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IAdmissionService>();
                var expiredCount = await service.ReleaseExpiredHoldsAsync(stoppingToken);

                if (expiredCount > 0)
                {
                    _logger.LogInformation("Expired {ExpiredCount} hold(s) and released seats.", expiredCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while expiring holds.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }

        _logger.LogInformation("Hold expiration service stopped.");
    }
}
