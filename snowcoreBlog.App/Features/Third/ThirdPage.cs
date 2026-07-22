using CustomShellMaui.Enum;
using CustomShellMaui.Models;
using MauiReactor;
using Nalu.Interfaces;
using Plugin.Maui.BottomSheet;
using Plugin.Maui.BottomSheet.Navigation;
using ReactorTheme.Styles;
using snowcoreBlog.App.Components;
using snowcoreBlog.App.Features.Home;
using snowcoreBlog.App.Features.Shared;
using snowcoreBlog.App.Resources;

namespace snowcoreBlog.App.Features.Third;

public partial class ThirdPage(INavigationService navigation, IBottomSheetNavigationService bottomSheetNavigation) : Component, IAppearingAware, ILeavingGuard, IDisposable
{
    private bool _disposed = false;

    private readonly INavigationService _navigation = navigation;

    private readonly IBottomSheetNavigationService _bottomSheetNavigation = bottomSheetNavigation;

    public override VisualNode Render() =>
        CustomContentPage(TranslationResources.SecondPageTitle, children:
            DelayedView(
                VStack(
                    VStack(
                        ButtonKit(() => TranslationResources.GoToFirstPageText)
                            .HorizontalOptions(LayoutOptions.Center)
                            .ThemeKey(ApplicationTheme.Primary)
                            .OnClicked(NavigateToFirstPageAsync),
                        ButtonKit(() => TranslationResources.BottomSheetText)
                            .HorizontalOptions(LayoutOptions.Center)
                            .ThemeKey(ApplicationTheme.Primary)
                            .OnClicked(OpenBottomSheetAsync)
                    )
                    .VCenter()
                )
            )
            .UseActivityIndicator(true)
        )
        .Set(PageTransitions.PageTransitionProperty, new Transitions
        {
            Root = new TransitionRoot
            {
                CurrentPage = TransitionType.FadeOut
            },
            Push = new Transition
            {
#if ANDROID
                DurationAndroid = 50,
#endif
                CurrentPage = TransitionType.FadeOut,
                NextPage = TransitionType.ScaleIn
            },
            Pop = new Transition
            {
#if ANDROID
                DurationAndroid = 50,
#endif
                CurrentPage = TransitionType.ScaleOut,
                NextPage = TransitionType.FadeIn
            },
        });

    public ValueTask OnAppearingAsync()
    {
        return ValueTask.CompletedTask;
    }
    
    public ValueTask<bool> CanLeaveAsync() =>
        ValueTask.FromResult(true);

    private Task NavigateToFirstPageAsync() =>
        _navigation.GoToAsync(Nalu.NavigationInfo.Navigation.Absolute().Root<HomePage>().WithIntent(new HomePageProps { PopInfo = TranslationResources.HelloWorld }));

    private Task OpenBottomSheetAsync() =>
        _bottomSheetNavigation.OpenBottomSheetAsync(() =>
            BottomSheet(new TestSheetContent()).HasHandle(false).Header(SheetHeader()).ShowHeader(true));

    private static BottomSheetHeader SheetHeader() =>
        new()
        {
            Style = new BottomSheetHeaderStyle()
            {
                CloseButtonTintColor = Colors.LightBlue
            },
            Content = new MauiControls.Border
            {
                AutomationId = AutomationIds.Handle,
                Margin = new(0, 10, 0, 10),
                WidthRequest = 40,
                HeightRequest = 7.5,
                Content = new MauiControls.BoxView()
                {
                    WidthRequest = 40,
                    Color = Colors.Orange,
                },
                StrokeShape = new MauiControls.Shapes.RoundRectangle()
                {
                    CornerRadius = new(20),
                },
                Stroke = Colors.Orange,
            }
        };

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