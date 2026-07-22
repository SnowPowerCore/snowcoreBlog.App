using MauiReactor;
using MauiReactor.Parameters;
using Mocale.Abstractions;
using Mocale.Translations;
using Nalu;
using snowcoreBlog.App.Features.Home;
using snowcoreBlog.App.Features.Settings;
using snowcoreBlog.App.Features.TabThree;
using snowcoreBlog.App.Features.TabTwo;

namespace snowcoreBlog.App.Features.BlogAppShell;

public partial class AppShell : Component
{
    private IParameter<AppShellSettings> _appShellSettings;

    [Inject]
    private readonly ITranslatorManager _translatorManager;

    protected override void OnMounted()
    {
        _appShellSettings = CreateParameter<AppShellSettings>();
        _appShellSettings.Value.CurrentCultureChanged += OnCurrentCultureChanged;

        base.OnMounted();
    }

    override protected void OnWillUnmount()
    {
        _appShellSettings.Value.CurrentCultureChanged -= OnCurrentCultureChanged;

        base.OnWillUnmount();
    }

    public override VisualNode Render() =>
        DeviceInfo.Current.Platform == DevicePlatform.WinUI
            ? RenderWindows()
            : RenderOther();

    private NavigationHost RenderWindows() =>
        NavigationHost(
            ShellContent()
                .Title(_translatorManager.Translate(TranslationKeys.HomeShellTitle))
                .Icon("icon_home.svg")
                .Set(Nalu.NavigationInfo.Navigation.PageTypeProperty, typeof(HomePage)),
            ShellContent()
                .Title(_translatorManager.Translate(TranslationKeys.TabTwoTitle))
                .Icon("icon_home.svg")
                .Set(Nalu.NavigationInfo.Navigation.PageTypeProperty, typeof(TabTwoPage)),
            ShellContent()
                .Title(_translatorManager.Translate(TranslationKeys.TabThreeTitle))
                .Icon("icon_home.svg")
                .Set(Nalu.NavigationInfo.Navigation.PageTypeProperty, typeof(TabThreePage)),
            ShellContent()
                .Title(_translatorManager.Translate(TranslationKeys.SettingsShellTitle))
                .Icon("icon_settings.svg")
                .Set(Nalu.NavigationInfo.Navigation.PageTypeProperty, typeof(SettingsPage))
        );

    private NavigationHost RenderOther() =>
        NavigationHost(
            TabBar(
                ShellContent()
                    .Title(_translatorManager.Translate(TranslationKeys.HomeShellTitle))
                    .Icon("icon_home.svg")
                    .Set(Nalu.NavigationInfo.Navigation.PageTypeProperty, typeof(HomePage)),
                ShellContent()
                    .Title(_translatorManager.Translate(TranslationKeys.TabTwoTitle))
                    .Icon("icon_home.svg")
                    .Set(Nalu.NavigationInfo.Navigation.PageTypeProperty, typeof(TabTwoPage)),
                ShellContent()
                    .Title(_translatorManager.Translate(TranslationKeys.TabThreeTitle))
                    .Icon("icon_home.svg")
                    .Set(Nalu.NavigationInfo.Navigation.PageTypeProperty, typeof(TabThreePage)),
                ShellContent()
                    .Title(_translatorManager.Translate(TranslationKeys.SettingsShellTitle))
                    .Icon("icon_settings.svg")
                    .Set(Nalu.NavigationInfo.Navigation.PageTypeProperty, typeof(SettingsPage))
            )
        );

    private void OnCurrentCultureChanged(object? sender, EventArgs e)
    {
        Invalidate();
    }
}