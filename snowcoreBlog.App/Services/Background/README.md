# Background Service Implementation for .NET MAUI

This document explains the background service implementation that complies with native platform requirements for stable background execution.

## Overview

The background service is implemented using Microsoft.Extensions.Hosting's `BackgroundService` class, which is registered as a `HostedService` in the MAUI application. It includes platform-specific implementations to ensure stable background execution across Android, iOS, MacCatalyst, and Windows.

**Key Feature**: The background work is executed within platform-specific background service scope, ensuring compliance with native platform requirements. On Android, work runs in a Foreground Service context; on iOS/MacCatalyst, work runs within a background task; on Windows, work runs in native background context.

## Architecture

### Components

1. **IPlatformBackgroundService** - Interface defining platform-specific background service operations
2. **SampleBackgroundService** - Main background service that runs continuously
3. **Platform-specific implementations:**
   - `AndroidPlatformBackgroundService` - Uses Foreground Service for stable execution
   - `iOSPlatformBackgroundService` - Uses Background Task API
   - `MacCatalystPlatformBackgroundService` - Uses Background Task API
   - `WindowsPlatformBackgroundService` - Runs natively in background

## Platform-Specific Details

### Android

- Uses **Foreground Service** with a persistent notification
- Background work is executed **within Foreground Service context** for maximum stability
- Requires `FOREGROUND_SERVICE` and `FOREGROUND_SERVICE_DATA_SYNC` permissions
- Service continues running even when the app is swiped away from recent tasks
- Shows a low-priority notification indicating background service is active
- Configured in `AndroidManifest.xml` with foreground service type
- The work action is passed to the Android foreground service and executed in its thread context

### iOS

- Uses **Background Tasks API** (BGTaskScheduler)
- Background work is executed **within iOS background task scope**
- Requires background modes configured in `Info.plist`
- System may limit execution time based on app state
- Background tasks are scheduled and executed by the system
- Supports `fetch` and `processing` background modes
- The work action is executed within `BeginBackgroundTask` scope to ensure proper iOS background execution

### MacCatalyst

- Uses **Background Task API** similar to iOS
- Background work is executed **within background task scope**
- Leverages `UIApplication.BeginBackgroundTask` for extended execution
- System manages background execution based on app state
- The work action is executed within the background task context to ensure proper MacCatalyst background execution

### Windows

- Background tasks run natively without special configuration
- Background work is executed **within platform service scope**
- Tasks continue executing when the app is minimized
- No additional permissions or manifest entries required
- The work action is executed in the native background context

## Usage

### Basic Setup

The background service is automatically registered and started when the application launches. The service runs in a loop every 30 seconds by default.

### Customizing Background Work

Modify the `SampleBackgroundService` class to add your custom background logic. Note that your work code runs within the platform-specific background service scope:

```csharp
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
                // Your custom background work here - runs in platform service scope
                await PerformCustomWork(ct);

                // Adjust delay as needed
                await Task.Delay(TimeSpan.FromMinutes(5), ct);
            }
        }, stoppingToken);
    }
    catch (OperationCanceledException)
    {
        _logger.LogInformation("SampleBackgroundService is stopping due to cancellation.");
    }
    finally
    {
        // Stop platform-specific service
        await _platformService.StopAsync(stoppingToken);
    }
}
```

**Important**: The work action passed to `StartAsync` is executed within the platform's background service context, ensuring native platform compliance.

### Adding New Background Services

To add additional background services:

1. Create a new class inheriting from `BackgroundService`
2. Register it in `MauiProgram.cs`:

```csharp
builder.Services.AddHostedService<YourCustomBackgroundService>();
```

## Configuration

### Service Registration

Located in `MauiProgram.cs`:

```csharp
// Platform-specific registration
#if ANDROID
    builder.Services.AddSingleton<IPlatformBackgroundService, AndroidPlatformBackgroundService>();
#elif IOS
    builder.Services.AddSingleton<IPlatformBackgroundService, iOSPlatformBackgroundService>();
// ... other platforms
#endif

// Main background service
builder.Services.AddHostedService<SampleBackgroundService>();
```

### Adjusting Execution Interval

Modify the delay in `SampleBackgroundService.ExecuteAsync`:

```csharp
// Current: 30 seconds
await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

// Example: 5 minutes
await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
```

## Platform-Specific Considerations

### Android

1. **Notification Requirement**: Foreground services must show a notification
2. **Battery Optimization**: Users may need to exempt app from battery optimization for extended execution
3. **Service Type**: Uses `dataSync` type, suitable for synchronization tasks

### iOS

1. **Limited Execution Time**: Background execution is limited to ~30 seconds unless using specific background modes
2. **System Control**: iOS system controls when background tasks execute
3. **App Store Review**: Ensure background task usage aligns with Apple's guidelines

### Windows

1. **No Restrictions**: Background tasks run freely
2. **Lifecycle**: Consider app lifecycle events for proper cleanup

## Logging

The service uses Microsoft.Extensions.Logging. Logs are configured in `MauiProgram.cs` and include:

- Service start/stop events
- Background work execution
- Error and exception details

View logs in the IDE output window or configure additional logging providers as needed.

## Troubleshooting

### Service Not Starting

1. Check platform-specific configurations (AndroidManifest.xml, Info.plist)
2. Verify all required permissions are granted
3. Review logs for error messages

### Service Stopping Unexpectedly

1. **Android**: Check if app is in battery optimization whitelist
2. **iOS**: Verify background modes are correctly configured
3. **All Platforms**: Check system resource constraints

### Performance Issues

1. Reduce execution frequency
2. Optimize background work operations
3. Consider using more efficient data structures

## Best Practices

1. **Keep Operations Light**: Background tasks should be short and efficient
2. **Handle Cancellations**: Always check `stoppingToken.IsCancellationRequested`
3. **Proper Cleanup**: Release resources in `StopAsync` or finalizers
4. **Logging**: Log important events for debugging
5. **Testing**: Test on real devices, not just simulators

## Dependencies

- `Microsoft.Extensions.Hosting` (v9.0.10)
- `Microsoft.Extensions.Logging`
- `Microsoft.Extensions.DependencyInjection`

## Additional Resources

- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Background Services in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/hosted-services)
- [Android Foreground Services](https://developer.android.com/guide/components/foreground-services)
- [iOS Background Tasks](https://developer.apple.com/documentation/backgroundtasks)

## License

This implementation is part of the snowcoreBlog.App project.