using ContentView = Microsoft.Maui.Controls.ContentView;

namespace snowcoreBlog.App.Controls;

public partial class InsetsView : ContentView
{
    public InsetsView(): base()
    {
        SetBinding(PaddingProperty, new Binding(nameof(Insets.InsetsThickness), source: Insets.Current));
    }
}

public partial class TopInsetView : ContentView
{
    public TopInsetView() : base()
    {
        SetBinding(PaddingProperty, new Binding(nameof(Insets.TopInsetThickness), source: Insets.Current));
    }
}

public partial class BottomInsetView : ContentView
{
    public BottomInsetView() : base()
    {
        SetBinding(PaddingProperty, new Binding(nameof(Insets.BottomInsetThickness), source: Insets.Current));
    }
}