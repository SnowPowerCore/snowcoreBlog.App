using Nalu;
using Nalu.Reactor;
using snowcoreBlog.App.Features.Home;
using snowcoreBlog.App.Features.Settings;
using snowcoreBlog.App.Features.TabThree;
using snowcoreBlog.App.Features.TabTwo;

namespace snowcoreBlog.App.Features.BlogAppShell;

public partial class AppShell : Component
{
    private readonly HomePage _homePage = new();
    private readonly SettingsPage _settingsPage = new();
    private readonly TabTwoPage _tabTwoPage = new();
    private readonly TabThreePage _tabThreePage = new();

    public override VisualNode Render() =>
        DeviceInfo.Current.Platform == DevicePlatform.WinUI
            ? RenderWindows()
            : RenderOther();

    private NaluReactorShell RenderWindows() =>
        AppShellControl(
            ShellContent()
                .Title(TranslationResources.HomeShellTitle)
                .Icon("icon_home.svg")
                .RenderContent<HomePage>(() => _homePage),
            ShellContent()
                .Title("Tab Two")
                .Icon("icon_home.svg")
                .RenderContent<TabTwoPage>(() => _tabTwoPage),
            ShellContent()
                .Title("Tab Three")
                .Icon("icon_home.svg")
                .RenderContent<TabThreePage>(() => _tabThreePage),
            ShellContent()
                .Title(TranslationResources.SettingsShellTitle)
                .Icon("icon_settings.svg")
                .RenderContent<SettingsPage>(() => _settingsPage)
        );

    private NaluReactorShell RenderOther() =>
        AppShellControl(
            TabBar(
                ShellContent()
                    .Title(TranslationResources.HomeShellTitle)
                    .Icon("icon_home.svg")
                    .RenderContent<HomePage>(() => _homePage),
                ShellContent()
                    .Title("Tab Two")
                    .Icon("icon_home.svg")
                    .RenderContent<TabTwoPage>(() => _tabTwoPage),
                ShellContent()
                    .Title("Tab Three")
                    .Icon("icon_home.svg")
                    .RenderContent<TabThreePage>(() => _tabThreePage),
                ShellContent()
                    .Title(TranslationResources.SettingsShellTitle)
                    .Icon("icon_settings.svg")
                    .RenderContent<SettingsPage>(() => _settingsPage)
            )
        );
}