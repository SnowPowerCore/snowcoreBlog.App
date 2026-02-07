using Nalu;
using Plugin.Maui.BottomSheet;
using Plugin.Maui.BottomSheet.Navigation;
using snowcoreBlog.App.Components;
using snowcoreBlog.App.Features.Home;
using snowcoreBlog.App.Features.Shared;

namespace snowcoreBlog.App.Features.Third;

public partial class ThirdPage : Component, IAppearingAware, ILeavingGuard, IDisposable
{
    private bool _disposed = false;

    private readonly INavigationService _navigation;

    private readonly IBottomSheetNavigationService _bottomSheetNavigation;

    public override VisualNode Render() =>
        ContentPage(
            VStack(
                VStack(
                    ButtonKit(() => TranslationResources.GoToFirstPageText)
                        .HorizontalOptions(LayoutOptions.Center)
                        .ThemeKey(ApplicationTheme.Primary)
                        .OnClicked(NavigateToFirstPageAsync),
                    ButtonKit(() => "Bottom Sheet")
                        .HorizontalOptions(LayoutOptions.Center)
                        .ThemeKey(ApplicationTheme.Primary)
                        .OnClicked(OpenBottomSheetAsync)
                )
                .VCenter(),
                BottomSheet()
            )
        )
        .BackgroundColor(Colors.Red)
        .Title(TranslationResources.SecondPageTitle);

    public ThirdPage(INavigationService navigation, IBottomSheetNavigationService bottomSheetNavigation)
    {
        _navigation = navigation;
        _bottomSheetNavigation = bottomSheetNavigation;
    }

    public ValueTask OnAppearingAsync()
    {
        return ValueTask.CompletedTask;
    }
    
    private Task OpenBottomSheetAsync() =>
        _bottomSheetNavigation.OpenBottomSheetAsync(() =>
            BottomSheet(new TestSheetContent()).HasHandle(false).Header(SheetHeader()).ShowHeader(true));

    private BottomSheetHeader SheetHeader()
    {
        var sheetHeader = new BottomSheetHeader
        {
            Style = new BottomSheetHeaderStyle()
            {
                CloseButtonTintColor = Colors.LightBlue
            },
            ContentTemplate = new DataTemplate(SheetHandle)
        };
        return sheetHeader;
    }

    private Microsoft.Maui.Controls.Border SheetHandle()
    {
        return new()
        {
            AutomationId = AutomationIds.Handle,
            Margin = new(0, 10, 0, 10),
            WidthRequest = 40,
            HeightRequest = 7.5,
            Content = new Microsoft.Maui.Controls.BoxView()
            {
                WidthRequest = 40,
                Color = Colors.Orange,
            },
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle()
            {
                CornerRadius = new(20),
            },
            Stroke = Colors.Orange,
        };
    }

    public ValueTask<bool> CanLeaveAsync() =>
        ValueTask.FromResult(true);

    private Task NavigateToFirstPageAsync() =>
        _navigation.GoToAsync(Nalu.Navigation.Absolute().Root<HomePage>().WithPropsDelegate<HomePageProps>(o => o.PopInfo = "Hello world!"));

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