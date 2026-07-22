using MauiReactor;
using MauiReactor.Internals;

namespace snowcoreBlog.App.Components;

[Scaffold(typeof(Controls.DelayedView))]
public partial class DelayedView
{
    protected override void OnAddChild(VisualNode widget, BindableObject childControl)
    {
        if (childControl is View)
        {
            Validate.EnsureNotNull(NativeControl);
            ShowView(NativeControl, childControl);
        }
        else
        {
            base.OnAddChild(widget, childControl);
        }
    }

    private static void ShowView(View view, BindableObject childControl)
    {
        var lazyView = view as Controls.DelayedView;
        if (lazyView is { IsLazyLoaded: false })
        {
            lazyView.View = (View)childControl;
            lazyView.LoadView();
        }

        view.IsVisible = true;
    }
}