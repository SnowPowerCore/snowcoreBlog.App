using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace snowcoreBlog.App.Services.Background;

/// <summary>
/// Base background service implementation for .NET MAUI applications.
/// This service runs continuously and performs background tasks.
/// </summary>
public partial class SampleBackgroundService(
    ILogger<SampleBackgroundService> logger,
    IPlatformBackgroundService platformService) : BackgroundService
{
    private readonly ILogger<SampleBackgroundService> _logger = logger;
    private readonly IPlatformBackgroundService _platformService = platformService;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SampleBackgroundService is starting.");

        try
        {
            // Start platform-specific service and execute work within its scope
            await _platformService.StartAsync(async (ct) =>
            {
                while (!ct.IsCancellationRequested)
                {
                    // Perform your background work here
                    _logger.LogInformation("SampleBackgroundService is running at: {time}", DateTimeOffset.Now);

                    // Example: Do some work
                    await PerformBackgroundWork(ct);

                    // Wait before next iteration
                    await Task.Delay(TimeSpan.FromSeconds(30), ct);
                }
            }, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("SampleBackgroundService is stopping due to cancellation.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SampleBackgroundService encountered an error.");
        }
        finally
        {
            // Stop platform-specific service
            await _platformService.StopAsync(stoppingToken);
        }
    }

    private async Task PerformBackgroundWork(CancellationToken cancellationToken)
    {
        // Replace this with your actual background work
        // For example: data synchronization, periodic checks, etc.
        await Task.Delay(100, cancellationToken);
        
        _logger.LogDebug("Background work completed.");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SampleBackgroundService is stopping.");
        await base.StopAsync(cancellationToken);
    }
}