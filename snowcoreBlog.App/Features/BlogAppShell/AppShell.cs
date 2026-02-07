using Nalu;
using snowcoreBlog.App.Features.Home;
using snowcoreBlog.App.Features.Settings;

namespace snowcoreBlog.App.Features.BlogAppShell;

public class AppShell : Component
{
    private readonly HomePage _homePage = new();
    private readonly SettingsPage _settingsPage = new();

    public override VisualNode Render() =>
        DeviceInfo.Current.Platform == DevicePlatform.WinUI
            ? RenderWindows()
            : RenderOther();

    private VisualNode RenderWindows() =>
        AppShellControl(
            ShellContent()
                .Title(TranslationResources.HomeShellTitle)
                .Icon("icon_home.svg")
                .RenderContent<HomePage>(() => _homePage),
            ShellContent()
                .Title(TranslationResources.SettingsShellTitle)
                .Icon("icon_settings.svg")
                .RenderContent<SettingsPage>(() => _settingsPage)
        );

    private VisualNode RenderOther() =>
        AppShellControl(
            TabBar(
                ShellContent()
                    .Title(TranslationResources.HomeShellTitle)
                    .Icon("icon_home.svg")
                    .RenderContent<HomePage>(() => _homePage),
                ShellContent()
                    .Title(TranslationResources.SettingsShellTitle)
                    .Icon("icon_settings.svg")
                    .RenderContent<SettingsPage>(() => _settingsPage)
            )
        );
}