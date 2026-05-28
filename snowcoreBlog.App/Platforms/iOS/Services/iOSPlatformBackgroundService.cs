using BackgroundTasks;
using Microsoft.Extensions.Logging;
using snowcoreBlog.App.Services.Background;
using UIKit;

namespace snowcoreBlog.App.Platforms.iOS.Services;

/// <summary>
/// iOS-specific implementation of platform background service.
/// Uses Background Task API for background execution.
/// </summary>
public class iOSPlatformBackgroundService : IPlatformBackgroundService
{
    private readonly ILogger<iOSPlatformBackgroundService> _logger;
    private const string BackgroundTaskIdentifier = "com.snowcore.blogapp.backgroundtask";
    private nint _backgroundTaskId;
    private Func<CancellationToken, Task>? _workAction;
    private CancellationTokenSource? _internalCts;

    public iOSPlatformBackgroundService(
        ILogger<iOSPlatformBackgroundService> logger)
    {
        _logger = logger;
        RegisterBackgroundTask();
    }

    public async Task StartAsync(Func<CancellationToken, Task> workAction, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting iOS platform background service.");

            // Store work action to be executed in background task context
            _workAction = workAction;
            _internalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Start background task and execute work within its scope
            _backgroundTaskId = UIApplication.SharedApplication.BeginBackgroundTask(() =>
            {
                // This is called when the system is about to terminate the background task
                _logger.LogWarning("iOS background task is about to expire.");
                _internalCts?.Cancel();
                UIApplication.SharedApplication.EndBackgroundTask(_backgroundTaskId);
                _backgroundTaskId = 0;
            });

            // Execute work within the background task scope
            if (_workAction != null)
            {
                await _workAction(_internalCts.Token);
            }
            
            _logger.LogInformation("iOS platform background service started successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start iOS platform background service.");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Stopping iOS platform background service.");

            _internalCts?.Cancel();
            _internalCts?.Dispose();

            if (_backgroundTaskId != 0)
            {
                UIApplication.SharedApplication.EndBackgroundTask(_backgroundTaskId);
                _backgroundTaskId = 0;
            }

            await Task.CompletedTask;
            
            _logger.LogInformation("iOS platform background service stopped successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop iOS platform background service.");
        }
    }

    private void RegisterBackgroundTask()
    {
        try
        {
            // Register background task for scheduled execution
            BGTaskScheduler.Shared.Register(
                BackgroundTaskIdentifier,
                null,
                HandleBackgroundTask
            );
            
            _logger.LogInformation("iOS background task registered successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register iOS background task.");
        }
    }

    private void HandleBackgroundTask(BGTask task)
    {
        // Schedule the next background task
        ScheduleBackgroundTask();

        // Perform background work
        _logger.LogInformation("iOS background task started.");

        // Set expiration handler
        task.ExpirationHandler = () =>
        {
            _logger.LogWarning("iOS background task is expiring.");
            task.SetTaskCompleted(success: false);
        };

        // Simulate background work
        Task.Delay(5000).ContinueWith(t =>
        {
            _logger.LogInformation("iOS background task completed.");
            task.SetTaskCompleted(success: true);
        });
    }

    private void ScheduleBackgroundTask()
    {
        try
        {
            var request = new BGProcessingTaskRequest(BackgroundTaskIdentifier)
            {
                RequiresNetworkConnectivity = false,
                RequiresExternalPower = false
            };

            BGTaskScheduler.Shared.Submit(request, out var error);
            
            if (error != null)
            {
                _logger.LogError($"Failed to schedule iOS background task: {error.LocalizedDescription}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule iOS background task.");
        }
    }
}