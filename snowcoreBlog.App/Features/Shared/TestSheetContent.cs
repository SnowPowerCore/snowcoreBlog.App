using MauiReactor;
using Plugin.Maui.BottomSheet.Navigation;
using ReactorTheme.Styles;
using snowcoreBlog.App.Components.Overriden;
using snowcoreBlog.App.Resources;

namespace snowcoreBlog.App.Features.Shared;

public partial class TestSheetContent : Component
{
    [Inject]
    private readonly IBottomSheetNavigationService _bottomSheetNavigation;

    protected override void OnMounted()
    {
        base.OnMounted();
    }

    protected override void OnWillUnmount()
    {
        base.OnWillUnmount();
    }

    public override VisualNode Render() =>
        ScrollView(
            VStack(
                Label(TranslationResources.TestContent)
                    .ThemeKey(ApplicationTheme.H3)
                    .Center(),

                Label(TranslationResources.HomeLoremExplain)
                    .Center(),

                ButtonKit(() => TranslationResources.HelloWorld)
                    .HorizontalOptions(LayoutOptions.Center)
                    .ThemeKey(ApplicationTheme.Primary)
                    .OnClicked(CloseSheetAsync)
            )
            .Spacing(20)
            .Padding(20)
        );

    private async Task CloseSheetAsync()
    {
        var result = await _bottomSheetNavigation.GoBackAsync();

        if (result.Success is false && result.Exception is not null)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to close bottom sheet: {result.Exception}");
        }
    }
}
