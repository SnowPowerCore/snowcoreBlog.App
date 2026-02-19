using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Microsoft.Extensions.Logging;
using snowcoreBlog.App.Services.Background;
using AndroidApplication = Android.App.Application;

namespace snowcoreBlog.App.Platforms.Android.Services;

/// <summary>
/// Android-specific implementation of platform background service.
/// Uses Foreground Service for stable background execution.
/// </summary>
public class AndroidPlatformBackgroundService(
    ILogger<AndroidPlatformBackgroundService> logger) : IPlatformBackgroundService
{
    private readonly ILogger<AndroidPlatformBackgroundService> _logger = logger;
    private readonly Context _context = AndroidApplication.Context;
    private bool _isServiceStarted;

    private const string ServiceChannelId = "snowcoreBlog.BackgroundService";
    private const string ServiceChannelName = "Background Service";
    private const int NotificationId = 1000;

    private Func<CancellationToken, Task>? _workAction;

    public async Task StartAsync(Func<CancellationToken, Task> workAction, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting Android platform background service.");

            // Store work action to be executed in foreground service
            _workAction = workAction;

            // Set parent service reference so foreground service can access work action
            AndroidBackgroundForegroundService.SetParentService(this);

            // Create notification channel for foreground service
            CreateNotificationChannel();

            // Start foreground service
            var intent = new Intent(_context, typeof(AndroidBackgroundForegroundService));
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                _context.StartForegroundService(intent);
            }
            else
            {
                _context.StartService(intent);
            }

            _isServiceStarted = true;
            
            _logger.LogInformation("Android platform background service started successfully.");
            
            // Wait indefinitely while foreground service runs the work
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Android platform background service.");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Stopping Android platform background service.");

            if (_isServiceStarted)
            {
                var intent = new Intent(_context, typeof(AndroidBackgroundForegroundService));
                _context.StopService(intent);
                _isServiceStarted = false;
            }

            await Task.CompletedTask;
            
            _logger.LogInformation("Android platform background service stopped successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop Android platform background service.");
        }
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(
                ServiceChannelId,
                ServiceChannelName,
                NotificationImportance.Low)
            {
                Description = "Keeps the background service running"
            };

            var notificationManager = _context.GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager?.CreateNotificationChannel(channel);
        }
    }

    /// <summary>
    /// Android Foreground Service implementation.
    /// This service runs in the foreground to prevent the system from killing it.
    /// The actual background work is executed within this service's context.
    /// </summary>
    [Service]
    public class AndroidBackgroundForegroundService : Service
    {
        private const string NotificationTitle = "Background Service";
        private const string NotificationText = "Running background tasks...";
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _serviceTask;
        private static AndroidPlatformBackgroundService? _parentService;

        public static void SetParentService(AndroidPlatformBackgroundService parentService)
        {
            _parentService = parentService;
        }

        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            StartForegroundService();
            StartServiceExecution();
            return StartCommandResult.Sticky;
        }

        private void StartForegroundService()
        {
            var notification = new NotificationCompat.Builder(this, ServiceChannelId)
                .SetContentTitle(NotificationTitle)
                .SetContentText(NotificationText)
                .SetSmallIcon(Resource.Mipmap.appicon)
                .SetOngoing(true)
                .SetPriority(NotificationCompat.PriorityLow)
                .Build();

            StartForeground(NotificationId, notification);
        }

        private void StartServiceExecution()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _serviceTask = Task.Run(async () =>
            {
                try
                {
                    // Execute the work action within the foreground service context
                    if (_parentService?._workAction != null)
                    {
                        await _parentService._workAction(_cancellationTokenSource.Token);
                    }
                }
                catch (System.OperationCanceledException)
                {
                    // Service was stopped
                }
                catch (Exception ex)
                {
                    _parentService?._logger.LogError(ex, "Error executing work in foreground service.");
                }
            }, _cancellationTokenSource.Token);
        }

        public override void OnTaskRemoved(Intent? rootIntent)
        {
            base.OnTaskRemoved(rootIntent);
            // Service continues running even when app is removed from recent tasks
        }

        public override IBinder? OnBind(Intent? intent)
        {
            return null;
        }

        public override void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            base.OnDestroy();
        }
    }
}