namespace snowcoreBlog.App.Services.Background;

/// <summary>
/// Interface for platform-specific background service implementations.
/// Each platform (Android, iOS, Windows, MacCatalyst) provides its own implementation
/// to handle platform-specific background execution requirements.
/// </summary>
public interface IPlatformBackgroundService
{
    /// <summary>
    /// Starts the platform-specific background service and executes work within the service scope.
    /// </summary>
    /// <param name="workAction">The action to execute within the background service scope.</param>
    /// <param name="cancellationToken">Cancellation token to stop the service.</param>
    Task StartAsync(Func<CancellationToken, Task> workAction, CancellationToken cancellationToken);

    /// <summary>
    /// Stops the platform-specific background service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop the service.</param>
    Task StopAsync(CancellationToken cancellationToken);
}
