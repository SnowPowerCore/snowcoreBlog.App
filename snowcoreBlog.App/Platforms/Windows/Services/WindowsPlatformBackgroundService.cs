using Microsoft.Extensions.Logging;
using snowcoreBlog.App.Services.Background;

namespace snowcoreBlog.App.Platforms.Windows.Services;

/// <summary>
/// Windows-specific implementation of platform background service.
/// Windows allows background tasks to run freely when the app is minimized.
/// </summary>
public class WindowsPlatformBackgroundService : IPlatformBackgroundService
{
    private readonly ILogger<WindowsPlatformBackgroundService> _logger;
    private bool _isServiceStarted;
    private CancellationTokenSource? _internalCts;

    public WindowsPlatformBackgroundService(
        ILogger<WindowsPlatformBackgroundService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(Func<CancellationToken, Task> workAction, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting Windows platform background service.");

            // Windows doesn't require special background service setup
            // Tasks can continue running when the app is minimized
            _isServiceStarted = true;
            _internalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Execute work within platform service scope
            await workAction(_internalCts.Token);
            
            _logger.LogInformation("Windows platform background service started successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Windows platform background service.");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Stopping Windows platform background service.");

            _internalCts?.Cancel();
            _internalCts?.Dispose();
            _isServiceStarted = false;

            await Task.CompletedTask;
            
            _logger.LogInformation("Windows platform background service stopped successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop Windows platform background service.");
        }
    }
}