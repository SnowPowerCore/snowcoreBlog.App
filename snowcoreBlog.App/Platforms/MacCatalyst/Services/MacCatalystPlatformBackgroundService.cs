using Microsoft.Extensions.Logging;
using snowcoreBlog.App.Services.Background;
using UIKit;

namespace snowcoreBlog.App.Platforms.MacCatalyst.Services;

/// <summary>
/// MacCatalyst-specific implementation of platform background service.
/// Uses Background Task API for background execution on macOS.
/// </summary>
public class MacCatalystPlatformBackgroundService(
    ILogger<MacCatalystPlatformBackgroundService> logger) : IPlatformBackgroundService
{
    private readonly ILogger<MacCatalystPlatformBackgroundService> _logger = logger;
    private const string BackgroundTaskIdentifier = "com.snowcore.blogapp.backgroundtask";
    private nint _backgroundTaskId;
    private Func<CancellationToken, Task>? _workAction;
    private CancellationTokenSource? _internalCts;

    public async Task StartAsync(Func<CancellationToken, Task> workAction, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting MacCatalyst platform background service.");

            // Store work action to be executed in background task context
            _workAction = workAction;
            _internalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Start background task and execute work within its scope
            _backgroundTaskId = UIApplication.SharedApplication.BeginBackgroundTask(() =>
            {
                // This is called when the system is about to terminate the background task
                _logger.LogWarning("MacCatalyst background task is about to expire.");
                _internalCts?.Cancel();
                UIApplication.SharedApplication.EndBackgroundTask(_backgroundTaskId);
                _backgroundTaskId = 0;
            });

            // Execute work within background task scope
            if (_workAction != null)
            {
                await _workAction(_internalCts.Token);
            }
            
            _logger.LogInformation("MacCatalyst platform background service started successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start MacCatalyst platform background service.");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Stopping MacCatalyst platform background service.");

            _internalCts?.Cancel();
            _internalCts?.Dispose();

            if (_backgroundTaskId != 0)
            {
                UIApplication.SharedApplication.EndBackgroundTask(_backgroundTaskId);
                _backgroundTaskId = 0;
            }

            await Task.CompletedTask;
            
            _logger.LogInformation("MacCatalyst platform background service stopped successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop MacCatalyst platform background service.");
        }
    }
}