using snowcoreBlog.App.Utils;
using ActivityIndicator = Microsoft.Maui.Controls.ActivityIndicator;
using ContentView = Microsoft.Maui.Controls.ContentView;

namespace snowcoreBlog.App.Controls;

public interface ILazyView
{
    View Content { get; set; }

    Color AccentColor { get; }

    bool IsLazyLoaded { get; }

    void LoadView();
}

public class DelayedView : ContentView, ILazyView, IDisposable
{
    private bool _disposed = false;

    public static readonly BindableProperty LoadingViewProperty = BindableProperty.Create(
        nameof(LoadingView),
        typeof(View),
        typeof(DelayedView),
        propertyChanged: LoadingViewChanged);

    public static readonly BindableProperty AccentColorProperty = BindableProperty.Create(
        nameof(AccentColor),
        typeof(Color),
        typeof(DelayedView),
        Colors.Magenta,
        propertyChanged: AccentColorChanged);

    public static readonly BindableProperty UseActivityIndicatorProperty = BindableProperty.Create(
        nameof(UseActivityIndicator),
        typeof(bool),
        typeof(DelayedView),
        false,
        propertyChanged: UseActivityIndicatorChanged);

    public static readonly BindableProperty ViewProperty = BindableProperty.Create(
        nameof(View),
        typeof(View),
        typeof(DelayedView),
        default(View));

    public View? LoadingView
    {
        get => (View?)GetValue(LoadingViewProperty);
        set => SetValue(LoadingViewProperty, value);
    }

    public bool UseActivityIndicator
    {
        get => (bool)GetValue(UseActivityIndicatorProperty);
        set => SetValue(UseActivityIndicatorProperty, value);
    }

    public Color AccentColor
    {
        get => (Color)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    public View View
    {
        get => (View)GetValue(ViewProperty);
        set => SetValue(ViewProperty, value);
    }

    public int DelayInMilliseconds { get; set; } = 200;

    public bool IsLazyLoaded { get; protected set; }

    protected override void OnBindingContextChanged()
    {
        if (Content != null && Content is not ActivityIndicator)
        {
            Content.BindingContext = BindingContext;
        }
    }

    private static void LoadingViewChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        if (newvalue is View loadingView && bindable is DelayedView lazyView)
        {
            lazyView.Content = loadingView;
        }
    }

    private static void AccentColorChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        var lazyView = (ILazyView)bindable;
        if (lazyView.Content is ActivityIndicator activityIndicator)
        {
            activityIndicator.Color = (Color)newvalue;
        }
    }

    private static void UseActivityIndicatorChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        var lazyView = (ILazyView)bindable;
        bool useActivityIndicator = (bool)newvalue;

        if (useActivityIndicator)
        {
            lazyView.Content = new ActivityIndicator
            {
                Color = lazyView.AccentColor,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsRunning = true,
            };
        }
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
                if (Content is IDisposable disposable)
                {
                    IsLazyLoaded = false;
                    disposable.Dispose();
                }
            }
            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    public void LoadView()
    {
        if (IsLazyLoaded)
        {
            return;
        }

        TaskMonitor.Create(
            async () =>
            {
                await Task.Delay(DelayInMilliseconds);
                if (IsLazyLoaded)
                {
                    return;
                }

                IsLazyLoaded = true;
                Content = View;
            });
    }
}