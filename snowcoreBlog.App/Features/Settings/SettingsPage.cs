using Nalu;
using snowcoreBlog.App.Features.Home;

namespace snowcoreBlog.App.Features.Settings;

public partial class SettingsPage : Component, IAppearingAware, IDisposable
{
    private bool _disposed = false;

    [Inject]
    private readonly INavigationService _navigation;

    public override VisualNode Render() =>
        CustomContentPage(title: TranslationResources.SettingsPageTitle, children:
            ButtonKit(() => TranslationResources.GoToRootPageText)
                .ThemeKey(ApplicationTheme.Primary)
                .OnClicked(NavigateToRootAsync)
        );
        
    public ValueTask OnAppearingAsync()
    {
        Console.WriteLine($"Navigated to {nameof(SettingsPage)}");
        return ValueTask.CompletedTask;
    }

    public Task NavigateToRootAsync() =>
        _navigation.GoToAsync(Nalu.Navigation.Absolute().Root<HomePage>());
        
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }
}