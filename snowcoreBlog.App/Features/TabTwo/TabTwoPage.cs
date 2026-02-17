using Nalu;

namespace snowcoreBlog.App.Features.TabTwo;

public partial class TabTwoPage : Component, IAppearingAware, IDisposable
{
    private bool _disposed = false;

    public override VisualNode Render() => CustomContentPage("Tab Two");
        
    public ValueTask OnAppearingAsync()
    {
        Console.WriteLine($"Navigated to {nameof(TabTwoPage)}");
        return ValueTask.CompletedTask;
    }
        
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